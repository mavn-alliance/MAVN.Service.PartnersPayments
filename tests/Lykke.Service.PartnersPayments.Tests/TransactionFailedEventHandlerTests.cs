using System.Threading.Tasks;
using Lykke.Logs;
using Lykke.Service.PartnersPayments.Domain.Enums;
using Lykke.Service.PartnersPayments.Domain.Models;
using Lykke.Service.PartnersPayments.Domain.Repositories;
using Lykke.Service.PartnersPayments.Domain.Services;
using Lykke.Service.PartnersPayments.DomainServices.RabbitMq.Handlers;
using Lykke.Service.PartnersPayments.MsSqlRepositories.Entities;
using Moq;
using Xunit;

namespace Lykke.Service.PartnersPayments.Tests
{
    public class TransactionFailedEventHandlerTests
    {
        private const string FakeOperationId = "opId";
        private const string FakePaymentRequestId = "pId";

        private readonly Mock<IPaymentRequestBlockchainRepository> _paymentRequestBlockchainRepoMock = new Mock<IPaymentRequestBlockchainRepository>();
        private readonly Mock<IPaymentsRepository> _paymentsRepositoryMock = new Mock<IPaymentsRepository>();
        private readonly Mock<IPaymentsStatusUpdater> _paymentsStatusUpdaterMock = new Mock<IPaymentsStatusUpdater>(MockBehavior.Strict);

        [Fact]
        public async Task HandleAsync_PaymentWithThisOperationIdDoesNotExist_PaymentsRepoNotCalled()
        {
            _paymentRequestBlockchainRepoMock.Setup(x => x.GetByOperationIdAsync(FakeOperationId))
                .ReturnsAsync((IPaymentRequestBlockchainData) null);

            var sut = CreateSutInstance();

            await sut.HandleAsync(FakeOperationId);

            _paymentsRepositoryMock.Verify(x => x.GetByPaymentRequestIdAsync(It.IsAny<string>()),Times.Never);
        }

        [Theory]
        [InlineData(PaymentRequestStatus.Created)]
        [InlineData(PaymentRequestStatus.RejectedByCustomer)]
        [InlineData(PaymentRequestStatus.TokensBurnFailed)]
        [InlineData(PaymentRequestStatus.TokensBurnSucceeded)]
        [InlineData(PaymentRequestStatus.TokensRefundFailed)]
        [InlineData(PaymentRequestStatus.TokensRefundSucceeded)]
        [InlineData(PaymentRequestStatus.TokensTransferFailed)]
        [InlineData(PaymentRequestStatus.TokensTransferSucceeded)]
        public async Task HandleAsync_PaymentIsNotInCorrectStatus_PaymentsStatusUpdaterNotCalled(PaymentRequestStatus status)
        {
            _paymentRequestBlockchainRepoMock.Setup(x => x.GetByOperationIdAsync(FakeOperationId))
                .ReturnsAsync(new PaymentRequestBlockchainEntity
                {
                    PaymentRequestId = FakePaymentRequestId
                });

            _paymentsRepositoryMock.Setup(x => x.GetByPaymentRequestIdAsync(FakePaymentRequestId))
                .ReturnsAsync(new PaymentModel
                {
                    Status = status
                });

            var sut = CreateSutInstance();

            await sut.HandleAsync(FakeOperationId);

            _paymentsStatusUpdaterMock.Verify();
        }

        [Fact]
        public async Task HandleAsync_PaymentIsInTokensTransferStartedStatus_TokensTransferFailAsyncCalled()
        {
            _paymentRequestBlockchainRepoMock.Setup(x => x.GetByOperationIdAsync(FakeOperationId))
                .ReturnsAsync(new PaymentRequestBlockchainEntity
                {
                    PaymentRequestId = FakePaymentRequestId
                });

            _paymentsRepositoryMock.Setup(x => x.GetByPaymentRequestIdAsync(FakePaymentRequestId))
                .ReturnsAsync(new PaymentModel
                {
                    Status = PaymentRequestStatus.TokensTransferStarted
                });

            _paymentsStatusUpdaterMock.Setup(x => x.TokensTransferFailAsync(FakePaymentRequestId))
                .ReturnsAsync(PaymentStatusUpdateErrorCodes.None)
                .Verifiable();

            var sut = CreateSutInstance();

            await sut.HandleAsync(FakeOperationId);

            _paymentsStatusUpdaterMock.Verify(x => x.TokensTransferFailAsync(FakePaymentRequestId), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_PaymentIsInTokensBurnStartedStatus_TokensBurnFailAsyncCalled()
        {
            _paymentRequestBlockchainRepoMock.Setup(x => x.GetByOperationIdAsync(FakeOperationId))
                .ReturnsAsync(new PaymentRequestBlockchainEntity
                {
                    PaymentRequestId = FakePaymentRequestId
                });

            _paymentsRepositoryMock.Setup(x => x.GetByPaymentRequestIdAsync(FakePaymentRequestId))
                .ReturnsAsync(new PaymentModel
                {
                    Status = PaymentRequestStatus.TokensBurnStarted
                });

            _paymentsStatusUpdaterMock.Setup(x => x.TokensBurnFailAsync(FakePaymentRequestId))
                .ReturnsAsync(PaymentStatusUpdateErrorCodes.None)
                .Verifiable();

            var sut = CreateSutInstance();

            await sut.HandleAsync(FakeOperationId);

            _paymentsStatusUpdaterMock.Verify(x => x.TokensBurnFailAsync(FakePaymentRequestId), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_PaymentIsInTokensRefundStartedStatus_TokensRefundFailAsyncCalled()
        {
            _paymentRequestBlockchainRepoMock.Setup(x => x.GetByOperationIdAsync(FakeOperationId))
                .ReturnsAsync(new PaymentRequestBlockchainEntity
                {
                    PaymentRequestId = FakePaymentRequestId
                });

            _paymentsRepositoryMock.Setup(x => x.GetByPaymentRequestIdAsync(FakePaymentRequestId))
                .ReturnsAsync(new PaymentModel
                {
                    Status = PaymentRequestStatus.TokensRefundStarted
                });

            _paymentsStatusUpdaterMock.Setup(x => x.TokensRefundFailAsync(FakePaymentRequestId))
                .ReturnsAsync(PaymentStatusUpdateErrorCodes.None)
                .Verifiable();

            var sut = CreateSutInstance();

            await sut.HandleAsync(FakeOperationId);

            _paymentsStatusUpdaterMock.Verify(x => x.TokensRefundFailAsync(FakePaymentRequestId), Times.Once);
        }

        private TransactionFailedEventHandler CreateSutInstance()
        {
            return new TransactionFailedEventHandler(
                _paymentRequestBlockchainRepoMock.Object,
                _paymentsRepositoryMock.Object,
                _paymentsStatusUpdaterMock.Object,
                EmptyLogFactory.Instance);
        }
    }
}
