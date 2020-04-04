using System;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.Service.CustomerProfile.Client;
using Lykke.Service.CustomerProfile.Client.Models.Enums;
using Lykke.Service.EligibilityEngine.Client;
using Lykke.Service.EligibilityEngine.Client.Enums;
using Lykke.Service.EligibilityEngine.Client.Models.ConversionRate.Requests;
using Lykke.Service.PartnerManagement.Client;
using MAVN.Service.PartnersPayments.Contract;
using MAVN.Service.PartnersPayments.Domain.Enums;
using MAVN.Service.PartnersPayments.Domain.Models;
using MAVN.Service.PartnersPayments.Domain.Repositories;
using MAVN.Service.PartnersPayments.Domain.Services;
using MAVN.Service.PartnersPayments.DomainServices.Utils;
using Lykke.Service.WalletManagement.Client;
using Lykke.Service.WalletManagement.Client.Enums;

namespace MAVN.Service.PartnersPayments.DomainServices
{
    public class PaymentsService : IPaymentsService
    {
        private readonly ICustomerProfileClient _customerProfileClient;
        private readonly IWalletManagementClient _walletManagementClient;
        private readonly IPaymentsRepository _paymentsRepository;
        private readonly ISettingsService _settingsService;
        private readonly IRabbitPublisher<PartnerPaymentRequestCreatedEvent> _paymentRequestCreatedPublisher;
        private readonly IPartnerManagementClient _partnerManagementClient;
        private readonly IPaymentsStatusUpdater _paymentsStatusUpdater;
        private readonly ILog _log;
        private readonly IEligibilityEngineClient _eligibilityEngineClient;
        private readonly string _tokenSymbol;

        public PaymentsService(
            ICustomerProfileClient customerProfileClient,
            IWalletManagementClient walletManagementClient,
            IPaymentsRepository paymentsRepository,
            ISettingsService settingsService,
            IRabbitPublisher<PartnerPaymentRequestCreatedEvent> paymentRequestCreatedPublisher,
            IPartnerManagementClient partnerManagementClient,
            IEligibilityEngineClient eligibilityEngineClient,
            IPaymentsStatusUpdater paymentsStatusUpdater,
            string tokenSymbol,
            ILogFactory logFactory)
        {
            _customerProfileClient = customerProfileClient;
            _walletManagementClient = walletManagementClient;
            _paymentsRepository = paymentsRepository;
            _settingsService = settingsService;
            _paymentRequestCreatedPublisher = paymentRequestCreatedPublisher;
            _partnerManagementClient = partnerManagementClient;
            _paymentsStatusUpdater = paymentsStatusUpdater;
            _tokenSymbol = tokenSymbol;
            _eligibilityEngineClient = eligibilityEngineClient ??
                                       throw new ArgumentNullException(nameof(eligibilityEngineClient));
            _log = logFactory.CreateLog(this);
        }

        public async Task<PaymentRequestResult> InitiatePartnerPaymentAsync(IPaymentRequest paymentRequest)
        {
            #region Validation

            if (string.IsNullOrEmpty(paymentRequest.CustomerId))
                throw new ArgumentNullException(nameof(paymentRequest.CustomerId));

            if (string.IsNullOrEmpty(paymentRequest.PartnerId))
                throw new ArgumentNullException(nameof(paymentRequest.PartnerId));

            if (string.IsNullOrEmpty(paymentRequest.Currency))
                throw new ArgumentNullException(nameof(paymentRequest.Currency));

            var isPartnerIdValidGuid = Guid.TryParse(paymentRequest.PartnerId, out var partnerId);

            if (!isPartnerIdValidGuid)
                return PaymentRequestResult.Failed(PaymentRequestErrorCodes.PartnerIdIsNotAValidGuid);

            var isCustomerIdValidGuid = Guid.TryParse(paymentRequest.CustomerId, out var customerGuid);

            if (!isCustomerIdValidGuid)
                return PaymentRequestResult.Failed(PaymentRequestErrorCodes.CustomerIdIsNotAValidGuid);

            //We can have either amount in Tokens or in Fiat
            if (paymentRequest.FiatAmount != null && paymentRequest.TokensAmount != null)
                return PaymentRequestResult.Failed(PaymentRequestErrorCodes.CannotPassBothFiatAndTokensAmount);

            //Fiat or Tokens Amount must be provided
            if (paymentRequest.FiatAmount == null && paymentRequest.TokensAmount == null)
                return PaymentRequestResult.Failed(PaymentRequestErrorCodes.EitherFiatOrTokensAmountShouldBePassed);

            if (paymentRequest.TokensAmount != null && paymentRequest.TokensAmount <= 0)
                return PaymentRequestResult.Failed(PaymentRequestErrorCodes.InvalidTokensAmount);

            if (paymentRequest.FiatAmount != null && paymentRequest.FiatAmount <= 0)
                return PaymentRequestResult.Failed(PaymentRequestErrorCodes.InvalidFiatAmount);

            if (paymentRequest.TotalBillAmount <= 0)
                return PaymentRequestResult.Failed(PaymentRequestErrorCodes.InvalidTotalBillAmount);

            var customerProfile = await _customerProfileClient.CustomerProfiles.GetByCustomerIdAsync(paymentRequest.CustomerId);

            if (customerProfile.ErrorCode == CustomerProfileErrorCodes.CustomerProfileDoesNotExist)
                return PaymentRequestResult.Failed(PaymentRequestErrorCodes.CustomerDoesNotExist);

            var customerWalletStatus =
                await _walletManagementClient.Api.GetCustomerWalletBlockStateAsync(paymentRequest.CustomerId);

            if (customerWalletStatus.Status == CustomerWalletActivityStatus.Blocked)
                return PaymentRequestResult.Failed(PaymentRequestErrorCodes.CustomerWalletBlocked);

            var partnerDetails = await _partnerManagementClient.Partners.GetByIdAsync(partnerId);

            if (partnerDetails == null)
                return PaymentRequestResult.Failed(PaymentRequestErrorCodes.PartnerDoesNotExist);

            if (!string.IsNullOrEmpty(paymentRequest.LocationId))
            {
                if (partnerDetails.Locations.All(x => x.Id.ToString() != paymentRequest.LocationId))
                    return PaymentRequestResult.Failed(PaymentRequestErrorCodes.NoSuchLocationForThisPartner);
            }

            #endregion

            var paymentRequestId = Guid.NewGuid().ToString();

            paymentRequest.PaymentRequestId = paymentRequestId;

            //Blockchain does not accept null
            if (paymentRequest.LocationId == null)
                paymentRequest.LocationId = "";

            var convertOptimalByPartnerModel = new ConvertOptimalByPartnerRequest
            {
                CustomerId = customerGuid,
                PartnerId = partnerId,
                Amount = paymentRequest.TokensAmount ?? paymentRequest.FiatAmount.Value,
                FromCurrency = paymentRequest.TokensAmount.HasValue ? _tokenSymbol : paymentRequest.Currency,
                ToCurrency = paymentRequest.TokensAmount.HasValue ? paymentRequest.Currency : _tokenSymbol 
            };

            var convertOptimalByPartnerResponse =
                await _eligibilityEngineClient.ConversionRate.ConvertOptimalByPartnerAsync(convertOptimalByPartnerModel);

            if (convertOptimalByPartnerResponse.ErrorCode != EligibilityEngineErrors.None)
            {
                if (convertOptimalByPartnerResponse.ErrorCode == EligibilityEngineErrors.PartnerNotFound)
                    return PaymentRequestResult.Failed(PaymentRequestErrorCodes.PartnerDoesNotExist);
                if (convertOptimalByPartnerResponse.ErrorCode == EligibilityEngineErrors.ConversionRateNotFound)
                    return PaymentRequestResult.Failed(PaymentRequestErrorCodes.InvalidTokensOrCurrencyRateInPartner);
            }

            paymentRequest.TokensToFiatConversionRate = convertOptimalByPartnerResponse.UsedRate;

            if (paymentRequest.FiatAmount == null)
                paymentRequest.FiatAmount = (decimal)convertOptimalByPartnerResponse.Amount;
            else
                paymentRequest.TokensAmount = convertOptimalByPartnerResponse.Amount;

            var now = DateTime.UtcNow;
            paymentRequest.CustomerActionExpirationTimestamp = paymentRequest.CustomerExpirationInSeconds.HasValue
                ? now.AddSeconds(paymentRequest.CustomerExpirationInSeconds.Value)
                : now.Add(_settingsService.GetRequestsExpirationPeriod());

            await _paymentsRepository.AddAsync(paymentRequest);

            await _paymentRequestCreatedPublisher.PublishAsync(new PartnerPaymentRequestCreatedEvent
            {
                PaymentRequestId = paymentRequestId,
                CustomerId = paymentRequest.CustomerId,
                PartnerId = paymentRequest.PartnerId,
                Timestamp = DateTime.UtcNow
            });
            return PaymentRequestResult.Succeeded(PaymentRequestStatus.Created, paymentRequestId);
        }

        public async Task<PaymentModel> GetPaymentDetailsByPaymentId(string paymentRequestId)
        {
            if (string.IsNullOrEmpty(paymentRequestId))
                throw new ArgumentNullException(nameof(paymentRequestId));

            var payment = await _paymentsRepository.GetByPaymentRequestIdAsync(paymentRequestId);

            if (payment == null)
                return null;

            payment.CustomerActionExpirationTimeLeftInSeconds = payment.Status == PaymentRequestStatus.Created
                ? CalculateCustomerActionExpirationTimeLeftInSeconds(payment.CustomerActionExpirationTimestamp)
                : 0;

            return payment;
        }

        public async Task<PaginatedPaymentsModel> GetPendingPaymentRequestsForCustomerAsync(string customerId, int currentPage, int pageSize)
        {
            var (skip, take) = PagingUtils.GetNextPageParameters(currentPage, pageSize);

            var (paymentRequests, totalCount) = await _paymentsRepository.GetPendingPaymentRequestsForCustomerAsync(customerId, skip, take);

            return new PaginatedPaymentsModel
            {
                CurrentPage = currentPage,
                PageSize = pageSize,
                PaymentRequests = paymentRequests,
                TotalCount = totalCount
            };
        }

        public async Task<PaginatedPaymentsModel> GetSucceededPaymentRequestsForCustomerAsync(string customerId, int currentPage, int pageSize)
        {
            var (skip, take) = PagingUtils.GetNextPageParameters(currentPage, pageSize);

            var (paymentRequests, totalCount) =
                await _paymentsRepository.GetSucceededPaymentRequestsForCustomerAsync(customerId, skip, take);

            return new PaginatedPaymentsModel
            {
                CurrentPage = currentPage,
                PageSize = pageSize,
                PaymentRequests = paymentRequests,
                TotalCount = totalCount
            };
        }

        public async Task<PaginatedPaymentsModel> GetFailedPaymentRequestsForCustomerAsync(string customerId, int currentPage, int pageSize)
        {
            var (skip, take) = PagingUtils.GetNextPageParameters(currentPage, pageSize);

            var (paymentRequests, totalCount) =
                await _paymentsRepository.GetFailedPaymentRequestsForCustomerAsync(customerId, skip, take);

            return new PaginatedPaymentsModel
            {
                CurrentPage = currentPage,
                PageSize = pageSize,
                PaymentRequests = paymentRequests,
                TotalCount = totalCount
            };
        }

        public async Task MarkPaymentsAsExpiredAsync(TimeSpan expirationPeriod)
        {
            var expirationDate = DateTime.UtcNow - expirationPeriod;
            var expiredPaymentsIds = await _paymentsRepository.GetExpiredPaymentsAsync(expirationDate);

            foreach (var expiredPaymentId in expiredPaymentsIds)
            {
                var error = await _paymentsStatusUpdater.StartExpireRefundAsync(expiredPaymentId);

                if (error != PaymentStatusUpdateErrorCodes.None)
                    _log.Warning("Failed to mark payment as expired",
                        context: new { error, expiredPaymentId });
            }
        }

        public async Task MarkRequestsAsExpiredAsync()
        {
            var expiredPaymentsIds = await _paymentsRepository.GetExpiredRequestsAsync();

            foreach (var expiredPaymentId in expiredPaymentsIds)
            {
                var error = await _paymentsStatusUpdater.ExpireRequestAsync(expiredPaymentId);

                if (error != PaymentStatusUpdateErrorCodes.None)
                    _log.Warning("Failed to mark payment request as expired",
                        context: new { error, expiredPaymentId });
            }
        }

        private int CalculateCustomerActionExpirationTimeLeftInSeconds(DateTime expirationDate)
        {
            var timeLeftInSeconds = (int)(expirationDate - DateTime.UtcNow).TotalSeconds;

            return timeLeftInSeconds >= 0 ? timeLeftInSeconds : 0;
        }
    }
}
