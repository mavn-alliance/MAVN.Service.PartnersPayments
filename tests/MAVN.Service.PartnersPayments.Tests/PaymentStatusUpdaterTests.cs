using System;
using System.Threading.Tasks;
using Lykke.Logs;
using Lykke.RabbitMqBroker.Publisher;
using MAVN.Service.EligibilityEngine.Client;
using MAVN.Service.EligibilityEngine.Client.Enums;
using MAVN.Service.EligibilityEngine.Client.Models.ConversionRate.Requests;
using MAVN.Service.EligibilityEngine.Client.Models.ConversionRate.Responses;
using MAVN.Service.PartnersPayments.Contract;
using MAVN.Service.PartnersPayments.Domain.Enums;
using MAVN.Service.PartnersPayments.Domain.Models;
using MAVN.Service.PartnersPayments.Domain.Repositories;
using MAVN.Service.PartnersPayments.Domain.Services;
using MAVN.Service.PartnersPayments.DomainServices;
using MAVN.Service.PartnersPayments.DomainServices.Common;
using MAVN.Service.PrivateBlockchainFacade.Client;
using MAVN.Service.PrivateBlockchainFacade.Client.Models;
using MAVN.Service.WalletManagement.Client;
using MAVN.Service.WalletManagement.Client.Enums;
using MAVN.Service.WalletManagement.Client.Models.Responses;
using Moq;
using Xunit;

namespace MAVN.Service.PartnersPayments.Tests
{
    public class PaymentStatusUpdaterTests
    {
        private const string FakeCustomerId = "86ad784a-6592-4512-a151-9c7d373fc714";
        private const string FakePaymentId = "pId";
        private const long ValidAmount = 100;
        private const string FakeEncodedData = "encodedData";
        private const string FakePartnersPaymentsAddress = "address";
        private const string FakeMasterWalletAddress = "wallet-address";
        private const string FakeOperationId = "2d738a3f-e877-4342-9fd8-7e75cc12866a";
        private const string FakePartnerId = "1749d1a2-9db8-4c7f-993a-0c1945d95960";
        private const string TokenSymbol = "MVN";

        private readonly Mock<IPaymentsRepository> _paymentsRepoMock = new Mock<IPaymentsRepository>();
        private readonly Mock<IPrivateBlockchainFacadeClient> _pbfClientMock = new Mock<IPrivateBlockchainFacadeClient>();
        private readonly Mock<IWalletManagementClient> _wmClientMock = new Mock<IWalletManagementClient>();
        private readonly Mock<IBlockchainEncodingService> _blockchainEncodingServiceMock = new Mock<IBlockchainEncodingService>();
        private readonly Mock<IPaymentRequestBlockchainRepository> _paymetRequestBlockchainRepoMock = new Mock<IPaymentRequestBlockchainRepository>();
        private readonly Mock<ISettingsService> _settingsServiceMock = new Mock<ISettingsService>();
        private readonly Mock<IEligibilityEngineClient> _eligibilityEngineClientMock = new Mock<IEligibilityEngineClient>();
        private readonly Mock<IRabbitPublisher<PartnersPaymentStatusUpdatedEvent>> _statusUpdatePublisher = new Mock<IRabbitPublisher<PartnersPaymentStatusUpdatedEvent>>();

        [Theory]
        [InlineData(null, "id")]
        [InlineData("id", null)]
        public async Task ApproveByCustomer_CustomerIdOrPaymentIdNull_ArgumentNullException(string paymentRequestId, string customerId)
        {
            var sut = CreateSutInstance();

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                sut.ApproveByCustomerAsync(paymentRequestId, ValidAmount, customerId));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public async Task ApproveByCustomer_InvalidSendingAmount_ErrorReturned(long sendingAmount)
        {
            var sut = CreateSutInstance();

            var result = await sut.ApproveByCustomerAsync(FakePaymentId, sendingAmount, FakeCustomerId);

            Assert.Equal(PaymentStatusUpdateErrorCodes.InvalidAmount, result);
        }

        [Fact]
        public async Task ApproveByCustomer_CustomerWalletIsBlocked_ErrorReturned()
        {
            _wmClientMock.Setup(x => x.Api.GetCustomerWalletBlockStateAsync(FakeCustomerId))
                .ReturnsAsync(new CustomerWalletBlockStatusResponse
                {
                    Status = CustomerWalletActivityStatus.Blocked
                });

            var sut = CreateSutInstance();

            var result = await sut.ApproveByCustomerAsync(FakePaymentId, ValidAmount, FakeCustomerId);

            Assert.Equal(PaymentStatusUpdateErrorCodes.CustomerWalletIsBlocked, result);
        }

        [Fact]
        public async Task ApproveByCustomer_PaymentDoesNotExist_ErrorReturned()
        {
            _wmClientMock.Setup(x => x.Api.GetCustomerWalletBlockStateAsync(FakeCustomerId))
                .ReturnsAsync(new CustomerWalletBlockStatusResponse
                {
                    Status = CustomerWalletActivityStatus.Active
                });

            _paymentsRepoMock.Setup(x => x.GetByPaymentRequestIdAsync(FakePaymentId))
                .ReturnsAsync((PaymentModel)null);

            var sut = CreateSutInstance();

            var result = await sut.ApproveByCustomerAsync(FakePaymentId, ValidAmount, FakeCustomerId);

            Assert.Equal(PaymentStatusUpdateErrorCodes.PaymentDoesNotExist, result);
        }

        [Fact]
        public async Task ApproveByCustomer_CustomerWhoIsTryingToApproveIsNotTheSameAsInTheRequest_ErrorReturned()
        {
            _wmClientMock.Setup(x => x.Api.GetCustomerWalletBlockStateAsync(FakeCustomerId))
                .ReturnsAsync(new CustomerWalletBlockStatusResponse
                {
                    Status = CustomerWalletActivityStatus.Active
                });

            _paymentsRepoMock.Setup(x => x.GetByPaymentRequestIdAsync(FakePaymentId))
                .ReturnsAsync(new PaymentModel { CustomerId = "a" });

            var sut = CreateSutInstance();

            var result = await sut.ApproveByCustomerAsync(FakePaymentId, ValidAmount, FakeCustomerId);

            Assert.Equal(PaymentStatusUpdateErrorCodes.CustomerIdDoesNotMatch, result);
        }

        [Fact]
        public async Task ApproveByCustomer_PaymentIsNotWithValidStatus_ErrorReturned()
        {
            _wmClientMock.Setup(x => x.Api.GetCustomerWalletBlockStateAsync(FakeCustomerId))
                .ReturnsAsync(new CustomerWalletBlockStatusResponse
                {
                    Status = CustomerWalletActivityStatus.Active
                });

            _paymentsRepoMock.Setup(x => x.GetByPaymentRequestIdAsync(FakePaymentId))
                .ReturnsAsync(new PaymentModel { CustomerId = FakeCustomerId, Status = PaymentRequestStatus.RejectedByCustomer });

            var sut = CreateSutInstance();

            var result = await sut.ApproveByCustomerAsync(FakePaymentId, ValidAmount, FakeCustomerId);

            Assert.Equal(PaymentStatusUpdateErrorCodes.PaymentIsInInvalidStatus, result);
        }

        [Fact]
        public async Task ApproveByCustomer_SendingAmountBiggerThanTokensAmount_ErrorReturned()
        {
            _wmClientMock.Setup(x => x.Api.GetCustomerWalletBlockStateAsync(FakeCustomerId))
                .ReturnsAsync(new CustomerWalletBlockStatusResponse
                {
                    Status = CustomerWalletActivityStatus.Active
                });

            _paymentsRepoMock.Setup(x => x.GetByPaymentRequestIdAsync(FakePaymentId))
                .ReturnsAsync(new PaymentModel
                    {
                        CustomerId = FakeCustomerId,
                        Status = PaymentRequestStatus.Created,
                        TokensAmount = 90
                    });

            var sut = CreateSutInstance();

            var result = await sut.ApproveByCustomerAsync(FakePaymentId, ValidAmount, FakeCustomerId);

            Assert.Equal(PaymentStatusUpdateErrorCodes.InvalidAmount, result);
        }

        [Theory]
        [InlineData(EligibilityEngineErrors.PartnerNotFound, PaymentStatusUpdateErrorCodes.PartnerDoesNotExist)]
        [InlineData(EligibilityEngineErrors.ConversionRateNotFound, PaymentStatusUpdateErrorCodes.InvalidTokensOrCurrencyRateInPartner)]
        public async Task ApproveByCustomer_ErrorFromEligibilityEngine_ErrorReturned
            (EligibilityEngineErrors eligibilityEngineError, PaymentStatusUpdateErrorCodes paymentStatusUpdateError)
        {
            _wmClientMock.Setup(x => x.Api.GetCustomerWalletBlockStateAsync(FakeCustomerId))
                .ReturnsAsync(new CustomerWalletBlockStatusResponse
                {
                    Status = CustomerWalletActivityStatus.Active
                });

            _paymentsRepoMock.Setup(x => x.GetByPaymentRequestIdAsync(FakePaymentId))
                .ReturnsAsync(new PaymentModel
                {
                    CustomerId = FakeCustomerId,
                    Status = PaymentRequestStatus.Created,
                    TokensAmount = ValidAmount,
                    PartnerId = FakePartnerId
                });

            _eligibilityEngineClientMock.Setup(x =>
                    x.ConversionRate.ConvertOptimalByPartnerAsync(It.IsAny<ConvertOptimalByPartnerRequest>()))
                .ReturnsAsync(new ConvertOptimalByPartnerResponse { ErrorCode = eligibilityEngineError});

            var sut = CreateSutInstance();

            var result = await sut.ApproveByCustomerAsync(FakePaymentId, ValidAmount, FakeCustomerId);

            Assert.Equal(paymentStatusUpdateError, result);
        }

        [Fact]
        public async Task ApproveByCustomer_ErrorFromPbf_ErrorReturned()
        {
            _wmClientMock.Setup(x => x.Api.GetCustomerWalletBlockStateAsync(FakeCustomerId))
                .ReturnsAsync(new CustomerWalletBlockStatusResponse
                {
                    Status = CustomerWalletActivityStatus.Active
                });

            _paymentsRepoMock.Setup(x => x.GetByPaymentRequestIdAsync(FakePaymentId))
                .ReturnsAsync(new PaymentModel
                {
                    CustomerId = FakeCustomerId,
                    Status = PaymentRequestStatus.Created,
                    TokensAmount = ValidAmount,
                    FiatAmount = ValidAmount,
                    PartnerId = FakePartnerId
                });

            _eligibilityEngineClientMock.Setup(x =>
                    x.ConversionRate.ConvertOptimalByPartnerAsync(It.IsAny<ConvertOptimalByPartnerRequest>()))
                .ReturnsAsync(new ConvertOptimalByPartnerResponse {Amount = 1});

            _blockchainEncodingServiceMock.Setup(x => x.EncodePaymentRequestData(It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(FakeEncodedData);

            _settingsServiceMock.Setup(x => x.GetPartnersPaymentsAddress())
                .Returns(FakePartnersPaymentsAddress);

            _pbfClientMock.Setup(x =>
                    x.GenericTransfersApi.GenericTransferAsync(It.IsAny<GenericTransferRequestModel>()))
                .ReturnsAsync(new TransferResponseModel
                {
                    Error = TransferError.NotEnoughFunds
                });

            var sut = CreateSutInstance();

            var result = await sut.ApproveByCustomerAsync(FakePaymentId, ValidAmount, FakeCustomerId);

            Assert.Equal(PaymentStatusUpdateErrorCodes.NotEnoughFunds, result);
        }

        [Fact]
        public async Task ApproveByCustomer_EverythingValid_SuccessfullyAccepted()
        {
            _wmClientMock.Setup(x => x.Api.GetCustomerWalletBlockStateAsync(FakeCustomerId))
                .ReturnsAsync(new CustomerWalletBlockStatusResponse
                {
                    Status = CustomerWalletActivityStatus.Active
                });

            _paymentsRepoMock.Setup(x => x.GetByPaymentRequestIdAsync(FakePaymentId))
                .ReturnsAsync(new PaymentModel
                {
                    CustomerId = FakeCustomerId,
                    Status = PaymentRequestStatus.Created,
                    TokensAmount = ValidAmount,
                    FiatAmount = ValidAmount,
                    PartnerId = FakePartnerId
                });

            _blockchainEncodingServiceMock.Setup(x => x.EncodePaymentRequestData(It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(FakeEncodedData);

            _settingsServiceMock.Setup(x => x.GetPartnersPaymentsAddress())
                .Returns(FakePartnersPaymentsAddress);

            _eligibilityEngineClientMock.Setup(x =>
                    x.ConversionRate.ConvertOptimalByPartnerAsync(It.IsAny<ConvertOptimalByPartnerRequest>()))
                .ReturnsAsync(new ConvertOptimalByPartnerResponse { Amount = 10 });

            _pbfClientMock.Setup(x =>
                    x.GenericTransfersApi.GenericTransferAsync(It.IsAny<GenericTransferRequestModel>()))
                .ReturnsAsync(new TransferResponseModel
                {
                    Error = TransferError.None,
                    OperationId = FakeOperationId
                });

            var sut = CreateSutInstance();

            var result = await sut.ApproveByCustomerAsync(FakePaymentId, ValidAmount, FakeCustomerId);

            _paymetRequestBlockchainRepoMock.Verify(x => x.UpsertAsync(FakePaymentId, FakeOperationId));
            Assert.Equal(PaymentStatusUpdateErrorCodes.None, result);
        }

        [Theory]
        [InlineData(null, "id")]
        [InlineData("id", null)]
        public async Task RejectByCustomer_CustomerIdOrPaymentIdNull_ArgumentNullException(string paymentRequestId, string customerId)
        {
            var sut = CreateSutInstance();

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                sut.RejectByCustomerAsync(paymentRequestId, customerId));
        }

        [Fact]
        public async Task RejectByCustomer_PaymentDoesNotExist_ErrorReturned()
        {
            _paymentsRepoMock.Setup(x => x.GetByPaymentRequestIdAsync(FakePaymentId))
                .ReturnsAsync((PaymentModel)null);

            var sut = CreateSutInstance();

            var result = await sut.RejectByCustomerAsync(FakePaymentId, FakeCustomerId);

            Assert.Equal(PaymentStatusUpdateErrorCodes.PaymentDoesNotExist, result);
        }

        [Fact]
        public async Task RejectByCustomer_CustomerWhoIsTryingToRejectIsNotTheSameAsInTheRequest_ErrorReturned()
        {
            _paymentsRepoMock.Setup(x => x.GetByPaymentRequestIdAsync(FakePaymentId))
                .ReturnsAsync(new PaymentModel { CustomerId = "a" });

            var sut = CreateSutInstance();

            var result = await sut.RejectByCustomerAsync(FakePaymentId, FakeCustomerId);

            Assert.Equal(PaymentStatusUpdateErrorCodes.CustomerIdDoesNotMatch, result);
        }

        [Fact]
        public async Task RejectByCustomer_PaymentIsNotWithValidStatus_ErrorReturned()
        {
            _paymentsRepoMock.Setup(x => x.GetByPaymentRequestIdAsync(FakePaymentId))
                .ReturnsAsync(new PaymentModel { CustomerId = FakeCustomerId, Status = PaymentRequestStatus.RejectedByCustomer });

            var sut = CreateSutInstance();

            var result = await sut.RejectByCustomerAsync(FakePaymentId, FakeCustomerId);

            Assert.Equal(PaymentStatusUpdateErrorCodes.PaymentIsInInvalidStatus, result);
        }

        [Fact]
        public async Task RejectByCustomer_EverythingValid_SuccessfullyRejected()
        {
            _paymentsRepoMock.Setup(x => x.GetByPaymentRequestIdAsync(FakePaymentId))
                .ReturnsAsync(new PaymentModel { CustomerId = FakeCustomerId, Status = PaymentRequestStatus.Created });

            var sut = CreateSutInstance();

            var result = await sut.RejectByCustomerAsync(FakePaymentId, FakeCustomerId);

            _paymentsRepoMock.Verify(x => x.SetStatusAsync(FakePaymentId, PaymentRequestStatus.RejectedByCustomer), Times.Once);
            Assert.Equal(PaymentStatusUpdateErrorCodes.None, result);
        }

        [Fact]
        public async Task TokensTransferSucceedAsync_PaymentIdNull_ArgumentNullException()
        {
            var sut = CreateSutInstance();

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                sut.TokensTransferSucceedAsync(null, DateTime.Now));
        }

        [Fact]
        public async Task TokensTransferSucceedAsync_PaymentDoesNotExist_ErrorReturned()
        {
            _paymentsRepoMock.Setup(x => x.GetByPaymentRequestIdAsync(FakePaymentId))
                .ReturnsAsync((PaymentModel)null);

            var sut = CreateSutInstance();

            var result = await sut.TokensTransferSucceedAsync(FakePaymentId, DateTime.Now);

            Assert.Equal(PaymentStatusUpdateErrorCodes.PaymentDoesNotExist, result);
        }

        [Fact]
        public async Task TokensTransferSucceedAsync_PaymentIsNotWithValidStatus_ErrorReturned()
        {
            _paymentsRepoMock.Setup(x => x.GetByPaymentRequestIdAsync(FakePaymentId))
                .ReturnsAsync(new PaymentModel { CustomerId = FakeCustomerId, Status = PaymentRequestStatus.RejectedByCustomer });

            var sut = CreateSutInstance();

            var result = await sut.TokensTransferSucceedAsync(FakePaymentId, DateTime.Now);

            Assert.Equal(PaymentStatusUpdateErrorCodes.PaymentIsInInvalidStatus, result);
        }

        [Fact]
        public async Task TokensTransferSucceedAsync_EverythingValid_SuccessfulOperation()
        {
            _paymentsRepoMock.Setup(x => x.GetByPaymentRequestIdAsync(FakePaymentId))
                .ReturnsAsync(new PaymentModel
                {
                    CustomerId = FakeCustomerId,
                    Status = PaymentRequestStatus.TokensTransferStarted
                });

            var sut = CreateSutInstance();

            var result = await sut.TokensTransferSucceedAsync(FakePaymentId, DateTime.Now);

            _paymentsRepoMock.Verify(x => x.UpdatePaymentAsync(It.IsAny<PaymentModel>()), Times.Once);
            Assert.Equal(PaymentStatusUpdateErrorCodes.None, result);
        }

        [Fact]
        public async Task TokensTransferFailAsync_PaymentIdNull_ArgumentNullException()
        {
            var sut = CreateSutInstance();

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                sut.TokensTransferFailAsync(null));
        }

        [Fact]
        public async Task TokensTransferFailAsync_PaymentDoesNotExist_ErrorReturned()
        {
            _paymentsRepoMock.Setup(x => x.GetByPaymentRequestIdAsync(FakePaymentId))
                .ReturnsAsync((PaymentModel)null);

            var sut = CreateSutInstance();

            var result = await sut.TokensTransferFailAsync(FakePaymentId);

            Assert.Equal(PaymentStatusUpdateErrorCodes.PaymentDoesNotExist, result);
        }

        [Fact]
        public async Task TokensTransferFailAsync_PaymentIsNotWithValidStatus_ErrorReturned()
        {
            _paymentsRepoMock.Setup(x => x.GetByPaymentRequestIdAsync(FakePaymentId))
                .ReturnsAsync(new PaymentModel { CustomerId = FakeCustomerId, Status = PaymentRequestStatus.RejectedByCustomer });

            var sut = CreateSutInstance();

            var result = await sut.TokensTransferFailAsync(FakePaymentId);

            Assert.Equal(PaymentStatusUpdateErrorCodes.PaymentIsInInvalidStatus, result);
        }

        [Fact]
        public async Task TokensTransferFailAsync_EverythingValid_SuccessfulOperation()
        {
            _paymentsRepoMock.Setup(x => x.GetByPaymentRequestIdAsync(FakePaymentId))
                .ReturnsAsync(new PaymentModel
                {
                    CustomerId = FakeCustomerId,
                    Status = PaymentRequestStatus.TokensTransferStarted
                });

            var sut = CreateSutInstance();

            var result = await sut.TokensTransferFailAsync(FakePaymentId);

            _paymentsRepoMock.Verify(x => x.SetStatusAsync(FakePaymentId, PaymentRequestStatus.TokensTransferFailed),
                Times.Once);
            Assert.Equal(PaymentStatusUpdateErrorCodes.None, result);
        }

        [Fact]
        public async Task TokensBurnSucceedAsync_PaymentIdNull_ArgumentNullException()
        {
            var sut = CreateSutInstance();

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                sut.TokensBurnSucceedAsync(null, DateTime.Now));
        }

        [Fact]
        public async Task TokensBurnSucceedAsync_PaymentDoesNotExist_ErrorReturned()
        {
            _paymentsRepoMock.Setup(x => x.GetByPaymentRequestIdAsync(FakePaymentId))
                .ReturnsAsync((PaymentModel)null);

            var sut = CreateSutInstance();

            var result = await sut.TokensBurnSucceedAsync(FakePaymentId, DateTime.Now);

            Assert.Equal(PaymentStatusUpdateErrorCodes.PaymentDoesNotExist, result);
        }

        [Fact]
        public async Task TokensBurnSucceedAsync_PaymentIsNotWithValidStatus_ErrorReturned()
        {
            _paymentsRepoMock.Setup(x => x.GetByPaymentRequestIdAsync(FakePaymentId))
                .ReturnsAsync(new PaymentModel { CustomerId = FakeCustomerId, Status = PaymentRequestStatus.RejectedByCustomer });

            var sut = CreateSutInstance();

            var result = await sut.TokensBurnSucceedAsync(FakePaymentId, DateTime.Now);

            Assert.Equal(PaymentStatusUpdateErrorCodes.PaymentIsInInvalidStatus, result);
        }

        [Fact]
        public async Task TokensBurnSucceedAsync_EverythingValid_SuccessfulOperation()
        {
            _paymentsRepoMock.Setup(x => x.GetByPaymentRequestIdAsync(FakePaymentId))
                .ReturnsAsync(new PaymentModel
                {
                    CustomerId = FakeCustomerId,
                    Status = PaymentRequestStatus.TokensBurnStarted
                });

            var sut = CreateSutInstance();

            var result = await sut.TokensBurnSucceedAsync(FakePaymentId, DateTime.Now);

            _paymentsRepoMock.Verify(x => x.UpdatePaymentAsync(It.IsAny<PaymentModel>()), Times.Once);
            Assert.Equal(PaymentStatusUpdateErrorCodes.None, result);
        }

        [Fact]
        public async Task TokensBurnFailAsync_PaymentIdNull_ArgumentNullException()
        {
            var sut = CreateSutInstance();

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                sut.TokensBurnFailAsync(null));
        }

        [Fact]
        public async Task TokensBurnFailAsync_PaymentDoesNotExist_ErrorReturned()
        {
            _paymentsRepoMock.Setup(x => x.GetByPaymentRequestIdAsync(FakePaymentId))
                .ReturnsAsync((PaymentModel)null);

            var sut = CreateSutInstance();

            var result = await sut.TokensBurnFailAsync(FakePaymentId);

            Assert.Equal(PaymentStatusUpdateErrorCodes.PaymentDoesNotExist, result);
        }

        [Fact]
        public async Task TokensBurnFailAsync_PaymentIsNotWithValidStatus_ErrorReturned()
        {
            _paymentsRepoMock.Setup(x => x.GetByPaymentRequestIdAsync(FakePaymentId))
                .ReturnsAsync(new PaymentModel { CustomerId = FakeCustomerId, Status = PaymentRequestStatus.RejectedByCustomer });

            var sut = CreateSutInstance();

            var result = await sut.TokensBurnFailAsync(FakePaymentId);

            Assert.Equal(PaymentStatusUpdateErrorCodes.PaymentIsInInvalidStatus, result);
        }

        [Fact]
        public async Task TokensBurnFailAsync_EverythingValid_SuccessfulOperation()
        {
            _paymentsRepoMock.Setup(x => x.GetByPaymentRequestIdAsync(FakePaymentId))
                .ReturnsAsync(new PaymentModel
                {
                    CustomerId = FakeCustomerId,
                    Status = PaymentRequestStatus.TokensBurnStarted
                });

            var sut = CreateSutInstance();

            var result = await sut.TokensBurnFailAsync(FakePaymentId);

            _paymentsRepoMock.Verify(x => x.SetStatusAsync(FakePaymentId, PaymentRequestStatus.TokensBurnFailed),
                Times.Once);
            Assert.Equal(PaymentStatusUpdateErrorCodes.None, result);
        }

        [Fact]
        public async Task TokensRefundFailAsync_PaymentIdNull_ArgumentNullException()
        {
            var sut = CreateSutInstance();

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                sut.TokensRefundFailAsync(null));
        }

        [Fact]
        public async Task TokensRefundFailAsync_PaymentDoesNotExist_ErrorReturned()
        {
            _paymentsRepoMock.Setup(x => x.GetByPaymentRequestIdAsync(FakePaymentId))
                .ReturnsAsync((PaymentModel)null);

            var sut = CreateSutInstance();

            var result = await sut.TokensRefundFailAsync(FakePaymentId);

            Assert.Equal(PaymentStatusUpdateErrorCodes.PaymentDoesNotExist, result);
        }

        [Fact]
        public async Task TokensRefundFailAsync_PaymentIsNotWithValidStatus_ErrorReturned()
        {
            _paymentsRepoMock.Setup(x => x.GetByPaymentRequestIdAsync(FakePaymentId))
                .ReturnsAsync(new PaymentModel { CustomerId = FakeCustomerId, Status = PaymentRequestStatus.RejectedByCustomer });

            var sut = CreateSutInstance();

            var result = await sut.TokensRefundFailAsync(FakePaymentId);

            Assert.Equal(PaymentStatusUpdateErrorCodes.PaymentIsInInvalidStatus, result);
        }

        [Fact]
        public async Task TokensRefundFailAsync_EverythingValid_SuccessfulOperation()
        {
            _paymentsRepoMock.Setup(x => x.GetByPaymentRequestIdAsync(FakePaymentId))
                .ReturnsAsync(new PaymentModel
                {
                    CustomerId = FakeCustomerId,
                    Status = PaymentRequestStatus.TokensRefundStarted
                });

            var sut = CreateSutInstance();

            var result = await sut.TokensRefundFailAsync(FakePaymentId);

            _paymentsRepoMock.Verify(x => x.SetStatusAsync(FakePaymentId, PaymentRequestStatus.TokensRefundFailed),
                Times.Once);
            Assert.Equal(PaymentStatusUpdateErrorCodes.None, result);
        }

        [Fact]
        public async Task TokensRefundFailAsync_RefundBecauseOfExpiration_SuccessfulOperation()
        {
            _paymentsRepoMock.Setup(x => x.GetByPaymentRequestIdAsync(FakePaymentId))
                .ReturnsAsync(new PaymentModel
                {
                    CustomerId = FakeCustomerId,
                    Status = PaymentRequestStatus.ExpirationTokensRefundStarted
                });

            var sut = CreateSutInstance();

            var result = await sut.TokensRefundFailAsync(FakePaymentId);

            _paymentsRepoMock.Verify(x => x.SetStatusAsync(FakePaymentId, PaymentRequestStatus.ExpirationTokensRefundFailed),
                Times.Once);
            Assert.Equal(PaymentStatusUpdateErrorCodes.None, result);
        }

        [Fact]
        public async Task TokensRefundSucceedAsync_PaymentIdNull_ArgumentNullException()
        {
            var sut = CreateSutInstance();

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                sut.TokensRefundSucceedAsync(null));
        }

        [Fact]
        public async Task TokensRefundSucceedAsync_PaymentDoesNotExist_ErrorReturned()
        {
            _paymentsRepoMock.Setup(x => x.GetByPaymentRequestIdAsync(FakePaymentId))
                .ReturnsAsync((PaymentModel)null);

            var sut = CreateSutInstance();

            var result = await sut.TokensRefundSucceedAsync(FakePaymentId);

            Assert.Equal(PaymentStatusUpdateErrorCodes.PaymentDoesNotExist, result);
        }

        [Fact]
        public async Task TokensRefundSucceedAsync_PaymentIsNotWithValidStatus_ErrorReturned()
        {
            _paymentsRepoMock.Setup(x => x.GetByPaymentRequestIdAsync(FakePaymentId))
                .ReturnsAsync(new PaymentModel { CustomerId = FakeCustomerId, Status = PaymentRequestStatus.RejectedByCustomer });

            var sut = CreateSutInstance();

            var result = await sut.TokensRefundSucceedAsync(FakePaymentId);

            Assert.Equal(PaymentStatusUpdateErrorCodes.PaymentIsInInvalidStatus, result);
        }

        [Fact]
        public async Task TokensRefundSucceedAsync_EverythingValid_SuccessfulOperation()
        {
            _paymentsRepoMock.Setup(x => x.GetByPaymentRequestIdAsync(FakePaymentId))
                .ReturnsAsync(new PaymentModel
                {
                    CustomerId = FakeCustomerId,
                    Status = PaymentRequestStatus.TokensRefundStarted
                });

            var sut = CreateSutInstance();

            var result = await sut.TokensRefundSucceedAsync(FakePaymentId);

            _paymentsRepoMock.Verify(x => x.SetStatusAsync(FakePaymentId, PaymentRequestStatus.TokensRefundSucceeded),
                Times.Once);
            Assert.Equal(PaymentStatusUpdateErrorCodes.None, result);
        }

        [Fact]
        public async Task TokensRefundSucceedAsync_RefundBecauseOfExpiration_SuccessfulOperation()
        {
            _paymentsRepoMock.Setup(x => x.GetByPaymentRequestIdAsync(FakePaymentId))
                .ReturnsAsync(new PaymentModel
                {
                    CustomerId = FakeCustomerId,
                    Status = PaymentRequestStatus.ExpirationTokensRefundStarted
                });

            var sut = CreateSutInstance();

            var result = await sut.TokensRefundSucceedAsync(FakePaymentId);

            _paymentsRepoMock.Verify(x => x.SetStatusAsync(FakePaymentId, PaymentRequestStatus.ExpirationTokensRefundSucceeded),
                Times.Once);
            Assert.Equal(PaymentStatusUpdateErrorCodes.None, result);
        }

        [Fact]
        public async Task ApproveByReceptionistAsync_PaymentIdNull_ArgumentNullException()
        {
            var sut = CreateSutInstance();

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                sut.ApproveByReceptionistAsync(null));
        }

        [Fact]
        public async Task ApproveByReceptionistAsync_PaymentDoesNotExist_ErrorReturned()
        {
            _paymentsRepoMock.Setup(x => x.GetByPaymentRequestIdAsync(FakePaymentId))
                .ReturnsAsync((PaymentModel)null);

            var sut = CreateSutInstance();

            var result = await sut.ApproveByReceptionistAsync(FakePaymentId);

            Assert.Equal(PaymentStatusUpdateErrorCodes.PaymentDoesNotExist, result);
        }

        [Fact]
        public async Task ApproveByReceptionistAsync_PaymentIsNotWithValidStatus_ErrorReturned()
        {
            _paymentsRepoMock.Setup(x => x.GetByPaymentRequestIdAsync(FakePaymentId))
                .ReturnsAsync(new PaymentModel { CustomerId = FakeCustomerId, Status = PaymentRequestStatus.RejectedByCustomer });

            var sut = CreateSutInstance();

            var result = await sut.ApproveByReceptionistAsync(FakePaymentId);

            Assert.Equal(PaymentStatusUpdateErrorCodes.PaymentIsInInvalidStatus, result);
        }

        [Fact]
        public async Task ApproveByReceptionistAsync_EverythingValid_SuccessfulOperation()
        {
            _paymentsRepoMock.Setup(x => x.GetByPaymentRequestIdAsync(FakePaymentId))
                .ReturnsAsync(new PaymentModel
                {
                    CustomerId = FakeCustomerId,
                    Status = PaymentRequestStatus.TokensTransferSucceeded,
                    PaymentRequestId = FakePaymentId,
                    Timestamp = DateTime.Now,
                    PartnerId = FakePartnerId
                });

            _blockchainEncodingServiceMock.Setup(x =>
                    x.EncodeAcceptRequestData(FakePartnerId, null, It.IsAny<long>(), FakeCustomerId, FakePaymentId))
                .Returns(FakeEncodedData);

            _settingsServiceMock.Setup(x => x.GetPartnersPaymentsAddress())
                .Returns(FakePartnersPaymentsAddress);

            _settingsServiceMock.Setup(x => x.GetMasterWalletAddress())
                .Returns(FakeMasterWalletAddress);

            _pbfClientMock.Setup(x => x.OperationsApi.AddGenericOperationAsync(It.IsAny<GenericOperationRequest>()))
                .ReturnsAsync(new GenericOperationResponse { OperationId = Guid.Parse(FakeOperationId) });

            var sut = CreateSutInstance();

            var result = await sut.ApproveByReceptionistAsync(FakePaymentId);

            _pbfClientMock.Verify(x =>
                x.OperationsApi.AddGenericOperationAsync(
                    It.Is<GenericOperationRequest>(o => o.Data == FakeEncodedData &&
                                                        o.SourceAddress == FakeMasterWalletAddress &&
                                                        o.TargetAddress == FakePartnersPaymentsAddress)));

            _paymentsRepoMock.Verify(x => x.SetStatusAsync(FakePaymentId, PaymentRequestStatus.TokensBurnStarted),
                Times.Once);
            _paymetRequestBlockchainRepoMock.Verify(x => x.UpsertAsync(FakePaymentId, It.IsAny<string>()));
            Assert.Equal(PaymentStatusUpdateErrorCodes.None, result);
        }

        [Fact]
        public async Task CancelByPartnerAsync_PaymentIdNull_ArgumentNullException()
        {
            var sut = CreateSutInstance();

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                sut.CancelByPartnerAsync(null));
        }

        [Fact]
        public async Task CancelByPartnerAsync_PaymentDoesNotExist_ErrorReturned()
        {
            _paymentsRepoMock.Setup(x => x.GetByPaymentRequestIdAsync(FakePaymentId))
                .ReturnsAsync((PaymentModel)null);

            var sut = CreateSutInstance();

            var result = await sut.CancelByPartnerAsync(FakePaymentId);

            Assert.Equal(PaymentStatusUpdateErrorCodes.PaymentDoesNotExist, result);
        }

        [Fact]
        public async Task CancelByPartnerAsync_PaymentIsNotWithValidStatus_ErrorReturned()
        {
            _paymentsRepoMock.Setup(x => x.GetByPaymentRequestIdAsync(FakePaymentId))
                .ReturnsAsync(new PaymentModel { CustomerId = FakeCustomerId, Status = PaymentRequestStatus.RejectedByCustomer });

            var sut = CreateSutInstance();

            var result = await sut.CancelByPartnerAsync(FakePaymentId);

            Assert.Equal(PaymentStatusUpdateErrorCodes.PaymentIsInInvalidStatus, result);
        }

        [Fact]
        public async Task CancelByPartnerAsync_EverythingValid_SuccessfulOperation()
        {
            _paymentsRepoMock.Setup(x => x.GetByPaymentRequestIdAsync(FakePaymentId))
                .ReturnsAsync(new PaymentModel
                {
                    CustomerId = FakeCustomerId,
                    Status = PaymentRequestStatus.TokensTransferSucceeded,
                    PaymentRequestId = FakePaymentId,
                    Timestamp = DateTime.Now,
                    PartnerId = FakePartnerId
                });

            _blockchainEncodingServiceMock.Setup(x =>
                    x.EncodeRejectRequestData(FakePartnerId, null, It.IsAny<long>(), FakeCustomerId, FakePaymentId))
                .Returns(FakeEncodedData);

            _settingsServiceMock.Setup(x => x.GetPartnersPaymentsAddress())
                .Returns(FakePartnersPaymentsAddress);

            _settingsServiceMock.Setup(x => x.GetMasterWalletAddress())
                .Returns(FakeMasterWalletAddress);

            _pbfClientMock.Setup(x => x.OperationsApi.AddGenericOperationAsync(It.IsAny<GenericOperationRequest>()))
                .ReturnsAsync(new GenericOperationResponse { OperationId = Guid.Parse(FakeOperationId) });

            var sut = CreateSutInstance();

            var result = await sut.CancelByPartnerAsync(FakePaymentId);

            _pbfClientMock.Verify(x =>
                x.OperationsApi.AddGenericOperationAsync(
                    It.Is<GenericOperationRequest>(o => o.Data == FakeEncodedData &&
                                                        o.SourceAddress == FakeMasterWalletAddress &&
                                                        o.TargetAddress == FakePartnersPaymentsAddress)));

            _paymentsRepoMock.Verify(x => x.SetStatusAsync(FakePaymentId, PaymentRequestStatus.TokensRefundStarted),
                Times.Once);
            _paymetRequestBlockchainRepoMock.Verify(x => x.UpsertAsync(FakePaymentId, It.IsAny<string>()));
            Assert.Equal(PaymentStatusUpdateErrorCodes.None, result);
        }

        [Fact]
        public async Task CancelByPartnerAsync_ForCreatedPayment_SuccessfulOperation()
        {
            _paymentsRepoMock.Setup(x => x.GetByPaymentRequestIdAsync(FakePaymentId))
                .ReturnsAsync(new PaymentModel
                {
                    CustomerId = FakeCustomerId,
                    Status = PaymentRequestStatus.Created,
                    PaymentRequestId = FakePaymentId,
                    Timestamp = DateTime.Now,
                    PartnerId = FakePartnerId
                });

            _blockchainEncodingServiceMock
                .Setup(x => x.EncodeRejectRequestData(FakePartnerId, null, It.IsAny<long>(), FakeCustomerId,
                    FakePaymentId)).Returns(FakeEncodedData);

            _settingsServiceMock.Setup(x => x.GetPartnersPaymentsAddress())
                .Returns(FakePartnersPaymentsAddress);

            _settingsServiceMock.Setup(x => x.GetMasterWalletAddress())
                .Returns(FakeMasterWalletAddress);

            _pbfClientMock.Setup(x => x.OperationsApi.AddGenericOperationAsync(It.IsAny<GenericOperationRequest>()))
                .ReturnsAsync(new GenericOperationResponse {OperationId = Guid.Parse(FakeOperationId)});

            var sut = CreateSutInstance();

            var result = await sut.CancelByPartnerAsync(FakePaymentId);

            _pbfClientMock.Verify(
                x => x.OperationsApi.AddGenericOperationAsync(It.Is<GenericOperationRequest>(o =>
                    o.Data == FakeEncodedData && o.SourceAddress == FakeMasterWalletAddress &&
                    o.TargetAddress == FakePartnersPaymentsAddress)), Times.Never);

            _paymentsRepoMock.Verify(x => x.SetStatusAsync(FakePaymentId, PaymentRequestStatus.CancelledByPartner),
                Times.Once);

            _paymetRequestBlockchainRepoMock.Verify(x => x.UpsertAsync(FakePaymentId, It.IsAny<string>()), Times.Never);

            Assert.Equal(PaymentStatusUpdateErrorCodes.None, result);
        }

        [Fact]
        public async Task StartExpireRefundAsync_PaymentIdNull_ArgumentNullException()
        {
            var sut = CreateSutInstance();

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                sut.StartExpireRefundAsync(null));
        }

        [Fact]
        public async Task StartExpireRefundAsync_PaymentDoesNotExist_ErrorReturned()
        {
            _paymentsRepoMock.Setup(x => x.GetByPaymentRequestIdAsync(FakePaymentId))
                .ReturnsAsync((PaymentModel)null);

            var sut = CreateSutInstance();

            var result = await sut.StartExpireRefundAsync(FakePaymentId);

            Assert.Equal(PaymentStatusUpdateErrorCodes.PaymentDoesNotExist, result);
        }

        [Fact]
        public async Task StartExpireRefundAsync_PaymentIsNotWithValidStatus_ErrorReturned()
        {
            _paymentsRepoMock.Setup(x => x.GetByPaymentRequestIdAsync(FakePaymentId))
                .ReturnsAsync(new PaymentModel { CustomerId = FakeCustomerId, Status = PaymentRequestStatus.RejectedByCustomer });

            var sut = CreateSutInstance();

            var result = await sut.StartExpireRefundAsync(FakePaymentId);

            Assert.Equal(PaymentStatusUpdateErrorCodes.PaymentIsInInvalidStatus, result);
        }

        [Fact]
        public async Task StartExpireRefundAsync_EverythingValid_SuccessfulOperation()
        {
            _paymentsRepoMock.Setup(x => x.GetByPaymentRequestIdAsync(FakePaymentId))
                .ReturnsAsync(new PaymentModel
                {
                    CustomerId = FakeCustomerId,
                    Status = PaymentRequestStatus.TokensTransferSucceeded,
                    PaymentRequestId = FakePaymentId,
                    Timestamp = DateTime.Now,
                    PartnerId = FakePartnerId
                });

            _blockchainEncodingServiceMock.Setup(x =>
                    x.EncodeRejectRequestData(FakePartnerId, null, It.IsAny<long>(), FakeCustomerId, FakePaymentId))
                .Returns(FakeEncodedData);

            _settingsServiceMock.Setup(x => x.GetPartnersPaymentsAddress())
                .Returns(FakePartnersPaymentsAddress);

            _settingsServiceMock.Setup(x => x.GetMasterWalletAddress())
                .Returns(FakeMasterWalletAddress);

            _pbfClientMock.Setup(x => x.OperationsApi.AddGenericOperationAsync(It.IsAny<GenericOperationRequest>()))
                .ReturnsAsync(new GenericOperationResponse { OperationId = Guid.Parse(FakeOperationId) });

            var sut = CreateSutInstance();

            var result = await sut.StartExpireRefundAsync(FakePaymentId);

            _pbfClientMock.Verify(x =>
                x.OperationsApi.AddGenericOperationAsync(
                    It.Is<GenericOperationRequest>(o => o.Data == FakeEncodedData &&
                                                        o.SourceAddress == FakeMasterWalletAddress &&
                                                        o.TargetAddress == FakePartnersPaymentsAddress)));

            _paymentsRepoMock.Verify(x => x.SetStatusAsync(FakePaymentId, PaymentRequestStatus.ExpirationTokensRefundStarted),
                Times.Once);
            _paymetRequestBlockchainRepoMock.Verify(x => x.UpsertAsync(FakePaymentId, It.IsAny<string>()));
            Assert.Equal(PaymentStatusUpdateErrorCodes.None, result);
        }

        [Fact]
        public async Task ExpireRequestAsync_PaymentIdNull_ArgumentNullException()
        {
            var sut = CreateSutInstance();

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                sut.ExpireRequestAsync(null));
        }

        [Fact]
        public async Task ExpireRequestAsync_PaymentDoesNotExist_ErrorReturned()
        {
            _paymentsRepoMock.Setup(x => x.GetByPaymentRequestIdAsync(FakePaymentId))
                .ReturnsAsync((PaymentModel)null);

            var sut = CreateSutInstance();

            var result = await sut.ExpireRequestAsync(FakePaymentId);

            Assert.Equal(PaymentStatusUpdateErrorCodes.PaymentDoesNotExist, result);
        }

        [Fact]
        public async Task ExpireRequestAsync_PaymentIsNotWithValidStatus_ErrorReturned()
        {
            _paymentsRepoMock.Setup(x => x.GetByPaymentRequestIdAsync(FakePaymentId))
                .ReturnsAsync(new PaymentModel { CustomerId = FakeCustomerId, Status = PaymentRequestStatus.RejectedByCustomer });

            var sut = CreateSutInstance();

            var result = await sut.ExpireRequestAsync(FakePaymentId);

            Assert.Equal(PaymentStatusUpdateErrorCodes.PaymentIsInInvalidStatus, result);
        }

        [Fact]
        public async Task ExpireRequestAsync_EverythingValid_SuccessfulOperation()
        {
            _paymentsRepoMock.Setup(x => x.GetByPaymentRequestIdAsync(FakePaymentId))
                .ReturnsAsync(new PaymentModel
                {
                    CustomerId = FakeCustomerId,
                    Status = PaymentRequestStatus.Created
                });

            var sut = CreateSutInstance();

            var result = await sut.ExpireRequestAsync(FakePaymentId);

            _paymentsRepoMock.Verify(x => x.SetStatusAsync(FakePaymentId, PaymentRequestStatus.RequestExpired),
                Times.Once);
            Assert.Equal(PaymentStatusUpdateErrorCodes.None, result);
        }

        private PaymentsStatusUpdater CreateSutInstance()
        {
            return new PaymentsStatusUpdater(
                _paymentsRepoMock.Object,
                _pbfClientMock.Object,
                _wmClientMock.Object,
                _blockchainEncodingServiceMock.Object,
                new TransactionScopeHandler(EmptyLogFactory.Instance),
                _paymetRequestBlockchainRepoMock.Object,
                _settingsServiceMock.Object,
                _statusUpdatePublisher.Object,
                _eligibilityEngineClientMock.Object,
                TokenSymbol);
        }
    }
}
