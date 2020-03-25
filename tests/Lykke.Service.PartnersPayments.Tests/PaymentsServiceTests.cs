using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Logs;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.Service.CustomerProfile.Client;
using Lykke.Service.CustomerProfile.Client.Models.Enums;
using Lykke.Service.CustomerProfile.Client.Models.Responses;
using Lykke.Service.EligibilityEngine.Client;
using Lykke.Service.EligibilityEngine.Client.Enums;
using Lykke.Service.EligibilityEngine.Client.Models.ConversionRate.Requests;
using Lykke.Service.EligibilityEngine.Client.Models.ConversionRate.Responses;
using Lykke.Service.PartnerManagement.Client;
using Lykke.Service.PartnerManagement.Client.Models.Location;
using Lykke.Service.PartnerManagement.Client.Models.Partner;
using Lykke.Service.PartnersPayments.Contract;
using Lykke.Service.PartnersPayments.Domain.Enums;
using Lykke.Service.PartnersPayments.Domain.Models;
using Lykke.Service.PartnersPayments.Domain.Repositories;
using Lykke.Service.PartnersPayments.Domain.Services;
using Lykke.Service.PartnersPayments.DomainServices;
using Lykke.Service.WalletManagement.Client;
using Lykke.Service.WalletManagement.Client.Enums;
using Lykke.Service.WalletManagement.Client.Models.Responses;
using Moq;
using Xunit;

namespace Lykke.Service.PartnersPayments.Tests
{
    public class PaymentsServiceTests
    {
        private const string FakePaymentId = "51e53a07-5eea-4a68-93af-f4be9ad2837e";
        private const string FakeCustomerId = "d952b7a2-c35d-429d-8a4c-083ec5d099cd";
        private const string FakePartnerId = "0b85b844-40e8-43fb-b3c3-dd58a40d4533";
        private const string FakeLocationId = "ca3e778c-93f4-4b75-b55a-652bf81c418b";
        private const string InvalidCurrency = "a";
        private const long ValidAmount = 10;
        private const long InvalidAmount = -1;
        private const string TokenSymbol = "MVN";
        private readonly HashSet<string> _allowedCurrencies = new HashSet<string> { "AED", "USD", "MVN" };

        private readonly Mock<ICustomerProfileClient> _customerProfileClientMock = new Mock<ICustomerProfileClient>();
        private readonly Mock<IWalletManagementClient> _walletManagementClientMock = new Mock<IWalletManagementClient>();
        private readonly Mock<IPartnerManagementClient> _partnerManagementClientMock = new Mock<IPartnerManagementClient>();
        private readonly Mock<IPaymentsRepository> _paymentsRepositoryMock = new Mock<IPaymentsRepository>();
        private readonly Mock<ISettingsService> _settingsService = new Mock<ISettingsService>();
        private readonly Mock<IPaymentsStatusUpdater> _paymentsStatusUpdaterMock = new Mock<IPaymentsStatusUpdater>();
        private readonly Mock<IRabbitPublisher<PartnerPaymentRequestCreatedEvent>> _publisherMock = new Mock<IRabbitPublisher<PartnerPaymentRequestCreatedEvent>>();
        private readonly Mock<IEligibilityEngineClient> _eligibilityEngineClientMock = new Mock<IEligibilityEngineClient>();

        [Theory]
        [InlineData(null, "1", "2")]
        [InlineData("1", "", "2")]
        [InlineData("1", "1", null)]
        public async Task TryToInitiatePartnerPayment_RequiredFieldIsMissing_ArgumentNullException
            (string customerId, string partnerId, string currency)
        {
            var sut = CreateSutInstance();

            await Assert.ThrowsAsync<ArgumentNullException>(() => sut.InitiatePartnerPaymentAsync(new PaymentRequest
            {
                CustomerId = customerId,
                PartnerId = partnerId,
                Currency = currency
            }));
        }

        [Fact]
        public async Task TryToInitiatePartnerPayment_BothFiatAndTokensAmountPassed_ErrorReturned()
        {
            var sut = CreateSutInstance();

            var result = await sut.InitiatePartnerPaymentAsync(new PaymentRequest
            {
                CustomerId = FakeCustomerId,
                PartnerId = FakePartnerId,
                Currency = _allowedCurrencies.First(),
                FiatAmount = ValidAmount,
                TokensAmount = ValidAmount
            });

            Assert.Equal(PaymentRequestErrorCodes.CannotPassBothFiatAndTokensAmount, result.Error);
        }

        [Fact]
        public async Task TryToInitiatePartnerPayment_NeitherFiatNorTokensAmountPassed_ErrorReturned()
        {

            var sut = CreateSutInstance();

            var result = await sut.InitiatePartnerPaymentAsync(new PaymentRequest
            {
                CustomerId = FakeCustomerId,
                PartnerId = FakePartnerId,
                Currency = _allowedCurrencies.First(),
                FiatAmount = null,
                TokensAmount = null
            });

            Assert.Equal(PaymentRequestErrorCodes.EitherFiatOrTokensAmountShouldBePassed, result.Error);
        }

        [Fact]
        public async Task TryToInitiatePartnerPayment_InvalidTokensAmount_ErrorReturned()
        {
            var sut = CreateSutInstance();

            var result = await sut.InitiatePartnerPaymentAsync(new PaymentRequest
            {
                CustomerId = FakeCustomerId,
                PartnerId = FakePartnerId,
                Currency = _allowedCurrencies.First(),
                FiatAmount = null,
                TokensAmount = InvalidAmount
            });

            Assert.Equal(PaymentRequestErrorCodes.InvalidTokensAmount, result.Error);
        }

        [Fact]
        public async Task TryToInitiatePartnerPayment_InvalidFiatAmount_ErrorReturned()
        {
            var sut = CreateSutInstance();

            var result = await sut.InitiatePartnerPaymentAsync(new PaymentRequest
            {
                CustomerId = FakeCustomerId,
                PartnerId = FakePartnerId,
                Currency = _allowedCurrencies.First(),
                FiatAmount = InvalidAmount,
                TokensAmount = null
            });

            Assert.Equal(PaymentRequestErrorCodes.InvalidFiatAmount, result.Error);
        }

        [Fact]
        public async Task TryToInitiatePartnerPayment_InvalidTotalBillAmount_ErrorReturned()
        {
            var sut = CreateSutInstance();

            var result = await sut.InitiatePartnerPaymentAsync(new PaymentRequest
            {
                CustomerId = FakeCustomerId,
                PartnerId = FakePartnerId,
                Currency = _allowedCurrencies.First(),
                FiatAmount = ValidAmount,
                TokensAmount = null,
                TotalBillAmount = InvalidAmount
            });

            Assert.Equal(PaymentRequestErrorCodes.InvalidTotalBillAmount, result.Error);
        }

        [Fact]
        public async Task TryToInitiatePartnerPayment_CustomerDoesNotExist_ErrorReturned()
        {
            _customerProfileClientMock.Setup(x => x.CustomerProfiles.GetByCustomerIdAsync(FakeCustomerId, It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(new CustomerProfileResponse
                {
                    ErrorCode = CustomerProfileErrorCodes.CustomerProfileDoesNotExist
                });

            var sut = CreateSutInstance();

            var result = await sut.InitiatePartnerPaymentAsync(new PaymentRequest
            {
                CustomerId = FakeCustomerId,
                PartnerId = FakePartnerId,
                Currency = _allowedCurrencies.First(),
                FiatAmount = ValidAmount,
                TokensAmount = null,
                TotalBillAmount = ValidAmount
            });

            Assert.Equal(PaymentRequestErrorCodes.CustomerDoesNotExist, result.Error);
        }

        [Fact]
        public async Task TryToInitiatePartnerPayment_CustomerWalletIsBlocked_ErrorReturned()
        {
            _customerProfileClientMock.Setup(x => x.CustomerProfiles.GetByCustomerIdAsync(FakeCustomerId, It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(new CustomerProfileResponse
                {
                    ErrorCode = CustomerProfileErrorCodes.None
                });

            _walletManagementClientMock.Setup(x => x.Api.GetCustomerWalletBlockStateAsync(FakeCustomerId))
                .ReturnsAsync(new CustomerWalletBlockStatusResponse
                {
                    Status = CustomerWalletActivityStatus.Blocked
                });

            var sut = CreateSutInstance();

            var result = await sut.InitiatePartnerPaymentAsync(new PaymentRequest
            {
                CustomerId = FakeCustomerId,
                PartnerId = FakePartnerId,
                Currency = _allowedCurrencies.First(),
                FiatAmount = ValidAmount,
                TokensAmount = null,
                TotalBillAmount = ValidAmount
            });

            Assert.Equal(PaymentRequestErrorCodes.CustomerWalletBlocked, result.Error);
        }

        [Fact]
        public async Task TryToInitiatePartnerPayment_PartnerDoesNotExist_ErrorReturned()
        {
            _partnerManagementClientMock.Setup(x => x.Partners.GetByIdAsync(Guid.Parse(FakePartnerId)))
                .ReturnsAsync((PartnerDetailsModel)null);

            _customerProfileClientMock.Setup(x => x.CustomerProfiles.GetByCustomerIdAsync(FakeCustomerId, It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(new CustomerProfileResponse
                {
                    ErrorCode = CustomerProfileErrorCodes.None
                });

            _walletManagementClientMock.Setup(x => x.Api.GetCustomerWalletBlockStateAsync(FakeCustomerId))
                .ReturnsAsync(new CustomerWalletBlockStatusResponse
                {
                    Status = CustomerWalletActivityStatus.Active
                });

            var sut = CreateSutInstance();

            var result = await sut.InitiatePartnerPaymentAsync(new PaymentRequest
            {
                CustomerId = FakeCustomerId,
                PartnerId = FakePartnerId,
                Currency = _allowedCurrencies.First(),
                FiatAmount = ValidAmount,
                TokensAmount = null,
                TotalBillAmount = ValidAmount
            });

            Assert.Equal(PaymentRequestErrorCodes.PartnerDoesNotExist, result.Error);
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(1, 0)]
        [InlineData(0, 0)]
        public async Task TryToInitiatePartnerPayment_InvalidCurrencyOrTokenRateInfoInPartner_ErrorReturned(int tokensRate, int currencyRate)
        {
            _partnerManagementClientMock.Setup(x => x.Partners.GetByIdAsync(Guid.Parse(FakePartnerId)))
                .ReturnsAsync(new PartnerDetailsModel
                {
                    AmountInTokens = tokensRate,
                    AmountInCurrency = currencyRate
                });

            _customerProfileClientMock.Setup(x => x.CustomerProfiles.GetByCustomerIdAsync(FakeCustomerId, It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(new CustomerProfileResponse
                {
                    ErrorCode = CustomerProfileErrorCodes.None
                });

            _walletManagementClientMock.Setup(x => x.Api.GetCustomerWalletBlockStateAsync(FakeCustomerId))
                .ReturnsAsync(new CustomerWalletBlockStatusResponse
                {
                    Status = CustomerWalletActivityStatus.Active
                });

            _eligibilityEngineClientMock.Setup(x =>
                    x.ConversionRate.ConvertOptimalByPartnerAsync(It.IsAny<ConvertOptimalByPartnerRequest>()))
                .ReturnsAsync(new ConvertOptimalByPartnerResponse()
                {
                    ErrorCode = EligibilityEngineErrors.ConversionRateNotFound
                });

            var sut = CreateSutInstance();

            var result = await sut.InitiatePartnerPaymentAsync(new PaymentRequest
            {
                CustomerId = FakeCustomerId,
                PartnerId = FakePartnerId,
                Currency = _allowedCurrencies.First(),
                FiatAmount = ValidAmount,
                TokensAmount = null,
                TotalBillAmount = ValidAmount
            });

            _eligibilityEngineClientMock.Verify(x =>
                    x.ConversionRate.ConvertOptimalByPartnerAsync(It.IsAny<ConvertOptimalByPartnerRequest>()), Times.Once);

            Assert.Equal(PaymentRequestErrorCodes.InvalidTokensOrCurrencyRateInPartner, result.Error);
        }

        [Fact]
        public async Task TryToInitiatePartnerPayment_LocationIsNotForThisPartner_ErrorReturned()
        {
            _partnerManagementClientMock.Setup(x => x.Partners.GetByIdAsync(Guid.Parse(FakePartnerId)))
                .ReturnsAsync(new PartnerDetailsModel
                {
                    AmountInTokens = 1,
                    AmountInCurrency = 1,
                    Locations = new List<LocationDetailsModel>()
                    {
                        new LocationDetailsModel
                        {
                            Id = Guid.Parse(FakeLocationId)
                        }
                    }
                });

            _customerProfileClientMock.Setup(x => x.CustomerProfiles.GetByCustomerIdAsync(FakeCustomerId, It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(new CustomerProfileResponse
                {
                    ErrorCode = CustomerProfileErrorCodes.None
                });

            _walletManagementClientMock.Setup(x => x.Api.GetCustomerWalletBlockStateAsync(FakeCustomerId))
                .ReturnsAsync(new CustomerWalletBlockStatusResponse
                {
                    Status = CustomerWalletActivityStatus.Active
                });

            var sut = CreateSutInstance();

            var result = await sut.InitiatePartnerPaymentAsync(new PaymentRequest
            {
                CustomerId = FakeCustomerId,
                PartnerId = FakePartnerId,
                Currency = _allowedCurrencies.First(),
                FiatAmount = ValidAmount,
                TokensAmount = null,
                TotalBillAmount = ValidAmount,
                LocationId = "NotExistingLocation"
            });

            Assert.Equal(PaymentRequestErrorCodes.NoSuchLocationForThisPartner, result.Error);
        }

        [Fact]
        public async Task TryToInitiatePartnerPayment_TokensAmountProvided_SuccessfullyCreatedAndConvertedFiatAmount()
        {
            _partnerManagementClientMock.Setup(x => x.Partners.GetByIdAsync(Guid.Parse(FakePartnerId)))
                .ReturnsAsync(new PartnerDetailsModel
                {
                    AmountInCurrency = 1,
                    AmountInTokens = 2
                });

            _customerProfileClientMock.Setup(x => x.CustomerProfiles.GetByCustomerIdAsync(FakeCustomerId, It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(new CustomerProfileResponse
                {
                    ErrorCode = CustomerProfileErrorCodes.None
                });

            _walletManagementClientMock.Setup(x => x.Api.GetCustomerWalletBlockStateAsync(FakeCustomerId))
                .ReturnsAsync(new CustomerWalletBlockStatusResponse
                {
                    Status = CustomerWalletActivityStatus.Active
                });

            var sut = CreateSutInstance();

            _eligibilityEngineClientMock.Setup(x =>
                    x.ConversionRate.ConvertOptimalByPartnerAsync(It.IsAny<ConvertOptimalByPartnerRequest>()))
                .ReturnsAsync(new ConvertOptimalByPartnerResponse()
                {
                    ErrorCode = EligibilityEngineErrors.None,
                    UsedRate = 12,
                    Amount = 5
                });

            var result = await sut.InitiatePartnerPaymentAsync(new PaymentRequest
            {
                CustomerId = FakeCustomerId,
                PartnerId = FakePartnerId,
                Currency = _allowedCurrencies.First(),
                FiatAmount = null,
                TokensAmount = ValidAmount,
                TotalBillAmount = ValidAmount,
            });

            _paymentsRepositoryMock.Verify(
                x => x.AddAsync(It.Is<IPaymentRequest>(p =>
                    p.FiatAmount == 5 && p.TokensAmount == ValidAmount && p.CustomerId == FakeCustomerId &&
                    p.PartnerId == FakePartnerId && p.Currency == _allowedCurrencies.First())), Times.Once);
            _publisherMock.Verify(x => x.PublishAsync(It.IsAny<PartnerPaymentRequestCreatedEvent>()), Times.Once);
            Assert.Equal(PaymentRequestErrorCodes.None, result.Error);
        }

        [Fact]
        public async Task TryToInitiatePartnerPayment_FiatAmountProvided_SuccessfullyCreatedAndConvertedTokensAmount()
        {
            _partnerManagementClientMock.Setup(x => x.Partners.GetByIdAsync(Guid.Parse(FakePartnerId)))
                .ReturnsAsync(new PartnerDetailsModel
                {
                    AmountInCurrency = 1,
                    AmountInTokens = 2
                });

            _customerProfileClientMock.Setup(x => x.CustomerProfiles.GetByCustomerIdAsync(FakeCustomerId, It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(new CustomerProfileResponse
                {
                    ErrorCode = CustomerProfileErrorCodes.None
                });

            _walletManagementClientMock.Setup(x => x.Api.GetCustomerWalletBlockStateAsync(FakeCustomerId))
                .ReturnsAsync(new CustomerWalletBlockStatusResponse
                {
                    Status = CustomerWalletActivityStatus.Active
                });

            _eligibilityEngineClientMock.Setup(x =>
                x.ConversionRate.ConvertOptimalByPartnerAsync(It.IsAny<ConvertOptimalByPartnerRequest>()))
                .ReturnsAsync(new ConvertOptimalByPartnerResponse()
                {
                    ErrorCode = EligibilityEngineErrors.None,
                    UsedRate = 12,
                    Amount = 20
                });

            var sut = CreateSutInstance();

            var result = await sut.InitiatePartnerPaymentAsync(new PaymentRequest
            {
                CustomerId = FakeCustomerId,
                PartnerId = FakePartnerId,
                Currency = "USD",
                FiatAmount = ValidAmount,
                TokensAmount = null,
                TotalBillAmount = ValidAmount,
            });

            _paymentsRepositoryMock.Verify(
                x => x.AddAsync(It.Is<IPaymentRequest>(p =>
                    p.FiatAmount == ValidAmount && p.TokensAmount == 20 && p.CustomerId == FakeCustomerId &&
                    p.PartnerId == FakePartnerId && p.Currency == "USD")), Times.Once);
            _publisherMock.Verify(x => x.PublishAsync(It.IsAny<PartnerPaymentRequestCreatedEvent>()), Times.Once);
            Assert.Equal(PaymentRequestErrorCodes.None, result.Error);
        }

        [Fact]
        public async Task GetPaymentDetailsById_IdIsNull_ArgumentNullException()
        {
            var sut = CreateSutInstance();

            await Assert.ThrowsAsync<ArgumentNullException>(() => sut.GetPaymentDetailsByPaymentId(null));
        }

        [Fact]
        public async Task GetPaymentDetailsById_PaymentDoesNotExist_NullReturned()
        {
            _paymentsRepositoryMock.Setup(x => x.GetByPaymentRequestIdAsync(FakePaymentId))
                .ReturnsAsync((PaymentModel)null);

            var sut = CreateSutInstance();

            var result = await sut.GetPaymentDetailsByPaymentId(FakePaymentId);

            Assert.Null(result);
        }

        [Theory]
        [InlineData(PaymentRequestStatus.CancelledByPartner)]
        [InlineData(PaymentRequestStatus.ExpirationTokensRefundFailed)]
        [InlineData(PaymentRequestStatus.ExpirationTokensRefundStarted)]
        [InlineData(PaymentRequestStatus.ExpirationTokensRefundSucceeded)]
        [InlineData(PaymentRequestStatus.RejectedByCustomer)]
        [InlineData(PaymentRequestStatus.RequestExpired)]
        [InlineData(PaymentRequestStatus.TokensBurnFailed)]
        [InlineData(PaymentRequestStatus.TokensBurnStarted)]
        [InlineData(PaymentRequestStatus.TokensBurnSucceeded)]
        [InlineData(PaymentRequestStatus.TokensRefundFailed)]
        [InlineData(PaymentRequestStatus.TokensRefundStarted)]
        [InlineData(PaymentRequestStatus.TokensRefundSucceeded)]
        [InlineData(PaymentRequestStatus.TokensTransferStarted)]
        [InlineData(PaymentRequestStatus.TokensTransferSucceeded)]
        [InlineData(PaymentRequestStatus.TokensTransferFailed)]
        public async Task GetPaymentDetailsById_StatusIsNotCreated_ZeroIsReturnedForCustomerExpirationTimeLeft(PaymentRequestStatus status)
        {
            _paymentsRepositoryMock.Setup(x => x.GetByPaymentRequestIdAsync(FakePaymentId))
                .ReturnsAsync(new PaymentModel
                {
                    Status = status
                });

            var sut = CreateSutInstance();

            var result = await sut.GetPaymentDetailsByPaymentId(FakePaymentId);

            Assert.Equal(0, result.CustomerActionExpirationTimeLeftInSeconds);
        }

        [Fact]
        public async Task MarkPaymentsAsExpiredAsync_TwoExpiredPayments_StatusUpdaterCalled()
        {

            var fakePaymentId = "2";
            var anotherFakePaymentId = "3";

            _paymentsRepositoryMock.Setup(x => x.GetExpiredPaymentsAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(new string[]
                {
                    fakePaymentId,
                    anotherFakePaymentId
                });

            var sut = CreateSutInstance();

            await sut.MarkPaymentsAsExpiredAsync(TimeSpan.FromMinutes(1));

            _paymentsStatusUpdaterMock.Verify(x => x.StartExpireRefundAsync(fakePaymentId), Times.Once);
            _paymentsStatusUpdaterMock.Verify(x => x.StartExpireRefundAsync(anotherFakePaymentId), Times.Once);
        }

        [Fact]
        public async Task MarkPaymentsAsExpiredAsync_TwoExpiredRequest_StatusUpdaterCalled()
        {

            var fakeRequestId = "2";
            var anotherFakeRequestId = "3";

            _paymentsRepositoryMock.Setup(x => x.GetExpiredRequestsAsync())
                .ReturnsAsync(new string[]
                {
                    fakeRequestId,
                    anotherFakeRequestId
                });

            var sut = CreateSutInstance();

            await sut.MarkRequestsAsExpiredAsync();

            _paymentsStatusUpdaterMock.Verify(x => x.ExpireRequestAsync(fakeRequestId), Times.Once);
            _paymentsStatusUpdaterMock.Verify(x => x.ExpireRequestAsync(anotherFakeRequestId), Times.Once);
        }

        private PaymentsService CreateSutInstance()
        {
            return new PaymentsService(
                _customerProfileClientMock.Object,
                _walletManagementClientMock.Object,
                _paymentsRepositoryMock.Object,
                _settingsService.Object,
                _publisherMock.Object,
                _partnerManagementClientMock.Object,
                _eligibilityEngineClientMock.Object,
                _paymentsStatusUpdaterMock.Object,
                TokenSymbol,
                EmptyLogFactory.Instance);
        }
    }
}
