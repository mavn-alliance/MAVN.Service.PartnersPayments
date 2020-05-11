using System;
using System.Threading.Tasks;
using MAVN.Numerics;
using Lykke.RabbitMqBroker.Publisher;
using MAVN.Service.EligibilityEngine.Client;
using MAVN.Service.EligibilityEngine.Client.Enums;
using MAVN.Service.EligibilityEngine.Client.Models.ConversionRate.Requests;
using MAVN.Service.EligibilityEngine.Client.Models.ConversionRate.Responses;
using MAVN.Service.PartnersPayments.Contract;
using MAVN.Service.PartnersPayments.Domain.Common;
using MAVN.Service.PartnersPayments.Domain.Enums;
using MAVN.Service.PartnersPayments.Domain.Extensions;
using MAVN.Service.PartnersPayments.Domain.Models;
using MAVN.Service.PartnersPayments.Domain.Repositories;
using MAVN.Service.PartnersPayments.Domain.Services;
using MAVN.Service.PrivateBlockchainFacade.Client;
using MAVN.Service.PrivateBlockchainFacade.Client.Models;
using MAVN.Service.WalletManagement.Client;
using MAVN.Service.WalletManagement.Client.Enums;

namespace MAVN.Service.PartnersPayments.DomainServices
{
    public class PaymentsStatusUpdater : IPaymentsStatusUpdater
    {
        private readonly IPaymentsRepository _paymentsRepository;
        private readonly IPrivateBlockchainFacadeClient _pbfClient;
        private readonly IWalletManagementClient _walletManagementClient;
        private readonly IBlockchainEncodingService _blockchainEncodingService;
        private readonly ITransactionScopeHandler _transactionScopeHandler;
        private readonly IPaymentRequestBlockchainRepository _paymentRequestBlockchainRepository;
        private readonly ISettingsService _settingsService;
        private readonly IRabbitPublisher<PartnersPaymentStatusUpdatedEvent> _statusUpdatePublisher;
        private readonly IEligibilityEngineClient _eligibilityEngineClient;
        private readonly string _tokenSymbol;

        public PaymentsStatusUpdater(
            IPaymentsRepository paymentsRepository,
            IPrivateBlockchainFacadeClient pbfClient,
            IWalletManagementClient walletManagementClient,
            IBlockchainEncodingService blockchainEncodingService,
            ITransactionScopeHandler transactionScopeHandler,
            IPaymentRequestBlockchainRepository paymentRequestBlockchainRepository,
            ISettingsService settingsService,
            IRabbitPublisher<PartnersPaymentStatusUpdatedEvent> statusUpdatePublisher,
            IEligibilityEngineClient eligibilityEngineClient,
            string tokenSymbol)
        {
            _paymentsRepository = paymentsRepository;
            _pbfClient = pbfClient;
            _walletManagementClient = walletManagementClient;
            _blockchainEncodingService = blockchainEncodingService;
            _transactionScopeHandler = transactionScopeHandler;
            _paymentRequestBlockchainRepository = paymentRequestBlockchainRepository;
            _settingsService = settingsService;
            _statusUpdatePublisher = statusUpdatePublisher;
            _eligibilityEngineClient = eligibilityEngineClient;
            _tokenSymbol = tokenSymbol;
        }

        public async Task<PaymentStatusUpdateErrorCodes> ApproveByCustomerAsync(string paymentRequestId, Money18 sendingAmount, string customerId)
        {
            if (string.IsNullOrEmpty(paymentRequestId))
                throw new ArgumentNullException(nameof(paymentRequestId));

            if (string.IsNullOrEmpty(customerId))
                throw new ArgumentNullException(nameof(customerId));

            if (sendingAmount <= 0)
                return PaymentStatusUpdateErrorCodes.InvalidAmount;

            var customerBlockStatusResponse = await _walletManagementClient.Api.GetCustomerWalletBlockStateAsync(customerId);
            if (customerBlockStatusResponse.Status != CustomerWalletActivityStatus.Active)
                return PaymentStatusUpdateErrorCodes.CustomerWalletIsBlocked;

            return await _transactionScopeHandler.WithTransactionAsync(async () =>
            {
                var payment = await _paymentsRepository.GetByPaymentRequestIdAsync(paymentRequestId);

                if (payment == null)
                    return PaymentStatusUpdateErrorCodes.PaymentDoesNotExist;

                if (payment.CustomerId != customerId)
                    return PaymentStatusUpdateErrorCodes.CustomerIdDoesNotMatch;

                if (payment.Status != PaymentRequestStatus.Created)
                    return PaymentStatusUpdateErrorCodes.PaymentIsInInvalidStatus;

                if (payment.TokensAmount < sendingAmount)
                    return PaymentStatusUpdateErrorCodes.InvalidAmount;

                var calculatedFiatAmountFromPartnerRate = await CalculateAmountFromPartnerRate(Guid.Parse(payment.CustomerId),
                    Guid.Parse(payment.PartnerId),
                    sendingAmount, _tokenSymbol, payment.Currency);

                if (calculatedFiatAmountFromPartnerRate.ErrorCode == EligibilityEngineErrors.PartnerNotFound)
                    return PaymentStatusUpdateErrorCodes.PartnerDoesNotExist;

                if (calculatedFiatAmountFromPartnerRate.ErrorCode == EligibilityEngineErrors.ConversionRateNotFound)
                    return PaymentStatusUpdateErrorCodes.InvalidTokensOrCurrencyRateInPartner;

                payment.Status = PaymentRequestStatus.TokensTransferStarted;
                payment.TokensSendingAmount = sendingAmount;
                payment.FiatSendingAmount = (decimal)calculatedFiatAmountFromPartnerRate.Amount;

                await _paymentsRepository.UpdatePaymentAsync(payment);

                await _statusUpdatePublisher.PublishAsync(new PartnersPaymentStatusUpdatedEvent
                {
                    PaymentRequestId = paymentRequestId,
                    Status = payment.Status.ToContractModel()
                });

                var encodedPaymentData = _blockchainEncodingService.EncodePaymentRequestData(payment.PartnerId,
                    payment.LocationId, payment.Timestamp.ConvertToLongMs(), payment.CustomerId,
                    payment.PaymentRequestId);

                var pbfTransferResponse = await _pbfClient.GenericTransfersApi.GenericTransferAsync(
                    new GenericTransferRequestModel
                    {
                        Amount = sendingAmount,
                        AdditionalData = encodedPaymentData,
                        RecipientAddress = _settingsService.GetPartnersPaymentsAddress(),
                        SenderCustomerId = customerId,
                        TransferId = paymentRequestId
                    });

                if (pbfTransferResponse.Error != TransferError.None)
                    return (PaymentStatusUpdateErrorCodes)pbfTransferResponse.Error;

                await _paymentRequestBlockchainRepository.UpsertAsync(paymentRequestId, pbfTransferResponse.OperationId);
                return PaymentStatusUpdateErrorCodes.None;
            });
        }

        public async Task<PaymentStatusUpdateErrorCodes> RejectByCustomerAsync(string paymentRequestId, string customerId)
        {
            if (string.IsNullOrEmpty(paymentRequestId))
                throw new ArgumentNullException(nameof(paymentRequestId));

            if (string.IsNullOrEmpty(customerId))
                throw new ArgumentNullException(nameof(customerId));

            return await _transactionScopeHandler.WithTransactionAsync(async () =>
            {
                var payment = await _paymentsRepository.GetByPaymentRequestIdAsync(paymentRequestId);

                if (payment == null)
                    return PaymentStatusUpdateErrorCodes.PaymentDoesNotExist;

                if (payment.CustomerId != customerId)
                    return PaymentStatusUpdateErrorCodes.CustomerIdDoesNotMatch;

                if (payment.Status != PaymentRequestStatus.Created)
                    return PaymentStatusUpdateErrorCodes.PaymentIsInInvalidStatus;

                const PaymentRequestStatus newStatus = PaymentRequestStatus.RejectedByCustomer;

                await _paymentsRepository.SetStatusAsync(paymentRequestId, newStatus);

                await _statusUpdatePublisher.PublishAsync(new PartnersPaymentStatusUpdatedEvent
                {
                    PaymentRequestId = paymentRequestId,
                    Status = newStatus.ToContractModel()
                });

                return PaymentStatusUpdateErrorCodes.None;
            });
        }

        public async Task<PaymentStatusUpdateErrorCodes> TokensTransferSucceedAsync(string paymentRequestId, DateTime timestamp)
        {
            if (string.IsNullOrEmpty(paymentRequestId))
                throw new ArgumentNullException(nameof(paymentRequestId));

            return await _transactionScopeHandler.WithTransactionAsync(async () =>
            {
                var payment = await _paymentsRepository.GetByPaymentRequestIdAsync(paymentRequestId);

                if (payment == null)
                    return PaymentStatusUpdateErrorCodes.PaymentDoesNotExist;

                if (payment.Status != PaymentRequestStatus.TokensTransferStarted)
                    return PaymentStatusUpdateErrorCodes.PaymentIsInInvalidStatus;

                payment.Status = PaymentRequestStatus.TokensTransferSucceeded;
                payment.TokensReserveTimestamp = timestamp;

                await _paymentsRepository.UpdatePaymentAsync(payment);

                await _statusUpdatePublisher.PublishAsync(new PartnersPaymentStatusUpdatedEvent
                {
                    PaymentRequestId = paymentRequestId,
                    Status = payment.Status.ToContractModel()
                });

                return PaymentStatusUpdateErrorCodes.None;
            });
        }

        public async Task<PaymentStatusUpdateErrorCodes> TokensBurnSucceedAsync(string paymentRequestId, DateTime timestamp)
        {
            if (string.IsNullOrEmpty(paymentRequestId))
                throw new ArgumentNullException(nameof(paymentRequestId));

            return await _transactionScopeHandler.WithTransactionAsync(async () =>
            {
                var payment = await _paymentsRepository.GetByPaymentRequestIdAsync(paymentRequestId);

                if (payment == null)
                    return PaymentStatusUpdateErrorCodes.PaymentDoesNotExist;

                if (payment.Status != PaymentRequestStatus.TokensBurnStarted)
                    return PaymentStatusUpdateErrorCodes.PaymentIsInInvalidStatus;

                payment.TokensBurnTimestamp = timestamp;
                payment.Status = PaymentRequestStatus.TokensBurnSucceeded;

                await _paymentsRepository.UpdatePaymentAsync(payment);

                await _statusUpdatePublisher.PublishAsync(new PartnersPaymentStatusUpdatedEvent
                {
                    PaymentRequestId = paymentRequestId,
                    Status = payment.Status.ToContractModel()
                });

                return PaymentStatusUpdateErrorCodes.None;
            });
        }

        public async Task<PaymentStatusUpdateErrorCodes> ApproveByReceptionistAsync(string paymentRequestId)
        {
            if (string.IsNullOrEmpty(paymentRequestId))
                throw new ArgumentNullException(nameof(paymentRequestId));

            return await _transactionScopeHandler.WithTransactionAsync(async () =>
            {
                var payment = await _paymentsRepository.GetByPaymentRequestIdAsync(paymentRequestId);

                if (payment == null)
                    return PaymentStatusUpdateErrorCodes.PaymentDoesNotExist;

                if (payment.Status != PaymentRequestStatus.TokensTransferSucceeded)
                    return PaymentStatusUpdateErrorCodes.PaymentIsInInvalidStatus;

                const PaymentRequestStatus newStatus = PaymentRequestStatus.TokensBurnStarted;

                await _paymentsRepository.SetStatusAsync(paymentRequestId, newStatus);

                await _statusUpdatePublisher.PublishAsync(new PartnersPaymentStatusUpdatedEvent
                {
                    PaymentRequestId = paymentRequestId,
                    Status = newStatus.ToContractModel()
                });

                var encodedData = _blockchainEncodingService.EncodeAcceptRequestData
                (payment.PartnerId, payment.LocationId, payment.Timestamp.ConvertToLongMs(), payment.CustomerId,
                    payment.PaymentRequestId);

                var pbfResponse = await _pbfClient.OperationsApi.AddGenericOperationAsync(new GenericOperationRequest
                {
                    Data = encodedData,
                    SourceAddress = _settingsService.GetMasterWalletAddress(),
                    TargetAddress = _settingsService.GetPartnersPaymentsAddress()
                });

                await _paymentRequestBlockchainRepository.UpsertAsync(paymentRequestId,
                    pbfResponse.OperationId.ToString());
                return PaymentStatusUpdateErrorCodes.None;
            });
        }

        public Task<PaymentStatusUpdateErrorCodes> TokensBurnFailAsync(string paymentRequestId)
            => ChangeStatusAsync(paymentRequestId, PaymentRequestStatus.TokensBurnStarted,
                PaymentRequestStatus.TokensBurnFailed);

        public Task<PaymentStatusUpdateErrorCodes> TokensTransferFailAsync(string paymentRequestId)
            => ChangeStatusAsync(paymentRequestId, PaymentRequestStatus.TokensTransferStarted,
                PaymentRequestStatus.TokensTransferFailed);

        public async Task<PaymentStatusUpdateErrorCodes> CancelByPartnerAsync(string paymentRequestId)
        {
            if (string.IsNullOrEmpty(paymentRequestId))
                throw new ArgumentNullException(nameof(paymentRequestId));

            return await _transactionScopeHandler.WithTransactionAsync(async () =>
            {
                var payment = await _paymentsRepository.GetByPaymentRequestIdAsync(paymentRequestId);

                if (payment == null)
                    return PaymentStatusUpdateErrorCodes.PaymentDoesNotExist;

                if (payment.Status != PaymentRequestStatus.TokensTransferSucceeded &&
                    payment.Status != PaymentRequestStatus.Created)
                {
                    return PaymentStatusUpdateErrorCodes.PaymentIsInInvalidStatus;
                }

                var status = payment.Status == PaymentRequestStatus.Created
                    ? PaymentRequestStatus.CancelledByPartner
                    : PaymentRequestStatus.TokensRefundStarted;

                await _paymentsRepository.SetStatusAsync(paymentRequestId, status);

                if (status == PaymentRequestStatus.TokensRefundStarted)
                    await BlockchainRefundAsync(paymentRequestId, payment);

                return PaymentStatusUpdateErrorCodes.None;
            });
        }

        public async Task<PaymentStatusUpdateErrorCodes> StartExpireRefundAsync(string paymentRequestId)
        {
            if (string.IsNullOrEmpty(paymentRequestId))
                throw new ArgumentNullException(nameof(paymentRequestId));

            return await _transactionScopeHandler.WithTransactionAsync(async () =>
            {
                var payment = await _paymentsRepository.GetByPaymentRequestIdAsync(paymentRequestId);

                if (payment == null)
                    return PaymentStatusUpdateErrorCodes.PaymentDoesNotExist;

                if (payment.Status != PaymentRequestStatus.TokensTransferSucceeded)
                    return PaymentStatusUpdateErrorCodes.PaymentIsInInvalidStatus;

                const PaymentRequestStatus newStatus = PaymentRequestStatus.ExpirationTokensRefundStarted;

                await _paymentsRepository.SetStatusAsync(paymentRequestId, newStatus);

                await _statusUpdatePublisher.PublishAsync(new PartnersPaymentStatusUpdatedEvent
                {
                    PaymentRequestId = paymentRequestId,
                    Status = newStatus.ToContractModel()
                });

                await BlockchainRefundAsync(paymentRequestId, payment);

                return PaymentStatusUpdateErrorCodes.None;
            });
        }

        public Task<PaymentStatusUpdateErrorCodes> TokensRefundSucceedAsync(string paymentRequestId)
            => TokensRefundChangeStatusAsync(paymentRequestId, PaymentRequestStatus.TokensRefundSucceeded,
                PaymentRequestStatus.ExpirationTokensRefundSucceeded);

        public Task<PaymentStatusUpdateErrorCodes> TokensRefundFailAsync(string paymentRequestId)
            => TokensRefundChangeStatusAsync(paymentRequestId, PaymentRequestStatus.TokensRefundFailed,
                PaymentRequestStatus.ExpirationTokensRefundFailed);

        public Task<PaymentStatusUpdateErrorCodes> ExpireRequestAsync(string paymentRequestId)
            => ChangeStatusAsync(paymentRequestId, PaymentRequestStatus.Created,
                PaymentRequestStatus.RequestExpired);

        private async Task BlockchainRefundAsync(string paymentRequestId, PaymentModel payment)
        {
            var encodedData = _blockchainEncodingService.EncodeRejectRequestData(payment.PartnerId, payment.LocationId,
                payment.Timestamp.ConvertToLongMs(), payment.CustomerId, payment.PaymentRequestId);

            var pbfResponse = await _pbfClient.OperationsApi.AddGenericOperationAsync(new GenericOperationRequest
            {
                Data = encodedData,
                SourceAddress = _settingsService.GetMasterWalletAddress(),
                TargetAddress = _settingsService.GetPartnersPaymentsAddress()
            });

            await _paymentRequestBlockchainRepository.UpsertAsync(paymentRequestId, pbfResponse.OperationId.ToString());
        }

        private async Task<PaymentStatusUpdateErrorCodes> TokensRefundChangeStatusAsync
            (string paymentRequestId, PaymentRequestStatus tokensRefundStatus, PaymentRequestStatus expirationTokensRefundStatus)
        {
            if (string.IsNullOrEmpty(paymentRequestId))
                throw new ArgumentNullException(nameof(paymentRequestId));

            return await _transactionScopeHandler.WithTransactionAsync(async () =>
            {
                var payment = await _paymentsRepository.GetByPaymentRequestIdAsync(paymentRequestId);

                if (payment == null)
                    return PaymentStatusUpdateErrorCodes.PaymentDoesNotExist;

                if (payment.Status != PaymentRequestStatus.TokensRefundStarted && payment.Status != PaymentRequestStatus.ExpirationTokensRefundStarted)
                    return PaymentStatusUpdateErrorCodes.PaymentIsInInvalidStatus;

                var newStatus = payment.Status == PaymentRequestStatus.TokensRefundStarted
                    ? tokensRefundStatus
                    : expirationTokensRefundStatus;

                await _paymentsRepository.SetStatusAsync(paymentRequestId, newStatus);

                await _statusUpdatePublisher.PublishAsync(new PartnersPaymentStatusUpdatedEvent
                {
                    PaymentRequestId = paymentRequestId,
                    Status = newStatus.ToContractModel()
                });

                return PaymentStatusUpdateErrorCodes.None;
            });
        }

        private async Task<PaymentStatusUpdateErrorCodes> ChangeStatusAsync
            (string paymentRequestId, PaymentRequestStatus requiredStatus, PaymentRequestStatus updateStatus)
        {
            if (string.IsNullOrEmpty(paymentRequestId))
                throw new ArgumentNullException(nameof(paymentRequestId));

            return await _transactionScopeHandler.WithTransactionAsync(async () =>
            {
                var payment = await _paymentsRepository.GetByPaymentRequestIdAsync(paymentRequestId);

                if (payment == null)
                    return PaymentStatusUpdateErrorCodes.PaymentDoesNotExist;

                if (payment.Status != requiredStatus)
                    return PaymentStatusUpdateErrorCodes.PaymentIsInInvalidStatus;

                await _paymentsRepository.SetStatusAsync(paymentRequestId, updateStatus);

                await _statusUpdatePublisher.PublishAsync(new PartnersPaymentStatusUpdatedEvent
                {
                    PaymentRequestId = paymentRequestId,
                    Status = updateStatus.ToContractModel()
                });

                return PaymentStatusUpdateErrorCodes.None;
            });
        }


        private async Task<ConvertOptimalByPartnerResponse> CalculateAmountFromPartnerRate
            (Guid customerId, Guid partnerId, Money18 amount, string fromCurrency, string toCurrency)
        {
            var convertOptimalByPartnerModel = new ConvertOptimalByPartnerRequest
            {
                CustomerId = customerId,
                PartnerId = partnerId,
                Amount = amount,
                FromCurrency = fromCurrency,
                ToCurrency = toCurrency
            };

            var convertOptimalByPartnerResponse =
                await _eligibilityEngineClient.ConversionRate.ConvertOptimalByPartnerAsync(convertOptimalByPartnerModel);

            return convertOptimalByPartnerResponse;
        }
    }
}
