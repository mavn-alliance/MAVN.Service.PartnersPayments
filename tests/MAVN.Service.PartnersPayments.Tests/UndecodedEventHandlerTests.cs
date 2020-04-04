using System;
using System.Threading.Tasks;
using Lykke.Logs;
using Lykke.RabbitMqBroker.Publisher;
using MAVN.Service.PartnersPayments.Contract;
using MAVN.Service.PartnersPayments.Domain.Common;
using MAVN.Service.PartnersPayments.Domain.Enums;
using MAVN.Service.PartnersPayments.Domain.Models;
using MAVN.Service.PartnersPayments.Domain.Repositories;
using MAVN.Service.PartnersPayments.Domain.Services;
using MAVN.Service.PartnersPayments.DomainServices.RabbitMq.Handlers;
using Moq;
using Xunit;

namespace MAVN.Service.PartnersPayments.Tests
{
    public class UndecodedEventHandlerTests
    {
        private const string FakePartnersPaymentsContractAddress = "address";
        private const string FakePaymentId = "pId";
        private const string FakeData = "data";
        private const long FakeAmount = 100;

        private readonly string[] _fakeTopics = { "topic1", "topic2" };
        private readonly Mock<IBlockchainEventDecoder> _eventDecoderMock = new Mock<IBlockchainEventDecoder>();
        private readonly Mock<IPaymentsStatusUpdater> _paymentStatusUpdaterMock = new Mock<IPaymentsStatusUpdater>(MockBehavior.Strict);
        private readonly Mock<ISettingsService> _settingsServiceMock = new Mock<ISettingsService>();
        private readonly Mock<IPaymentsRepository> _paymentsRepoMock = new Mock<IPaymentsRepository>();
        private readonly Mock<IRabbitPublisher<PartnersPaymentTokensReservedEvent>> _paymentTokensReservedPublisher =
            new Mock<IRabbitPublisher<PartnersPaymentTokensReservedEvent>>();
        private readonly Mock<IRabbitPublisher<PartnersPaymentProcessedEvent>> _paymentProcessedPublisher =
            new Mock<IRabbitPublisher<PartnersPaymentProcessedEvent>>();

        [Fact]
        public async Task HandleAsync_ContractAddressIsNotPartnerPaymentsOne_EventDecoderNotCalled()
        {
            _settingsServiceMock.Setup(x => x.GetPartnersPaymentsAddress())
                .Returns(FakePartnersPaymentsContractAddress);

            var sut = CreateSutInstance();

            await sut.HandleAsync(_fakeTopics, FakeData, "AddressOfNoInterest", DateTime.UtcNow);

            _eventDecoderMock.Verify(x => x.GetEventType(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task HandleAsync_EventIsWithUnknownForUsType_StatusUpdaterNotCalled()
        {
            _settingsServiceMock.Setup(x => x.GetPartnersPaymentsAddress())
                .Returns(FakePartnersPaymentsContractAddress);

            _eventDecoderMock.Setup(x => x.GetEventType(_fakeTopics[0]))
                .Returns(BlockchainEventType.Unknown);

            var sut = CreateSutInstance();

            await sut.HandleAsync(_fakeTopics, FakeData, FakePartnersPaymentsContractAddress, DateTime.UtcNow);

            _paymentStatusUpdaterMock.Verify();
        }

        [Fact]
        public async Task HandleAsync_EventIsPaymentReceived_ErrorWhenChangingTheStatus_PublisherNotCalled()
        {
            _settingsServiceMock.Setup(x => x.GetPartnersPaymentsAddress())
                .Returns(FakePartnersPaymentsContractAddress);

            _eventDecoderMock.Setup(x => x.GetEventType(_fakeTopics[0]))
                .Returns(BlockchainEventType.TransferReceived);

            _eventDecoderMock.Setup(x => x.DecodeTransferReceivedEvent(_fakeTopics, FakeData))
                .Returns(new PaymentProcessedByPartnerModel { PaymentRequestId = FakePaymentId });

            _paymentStatusUpdaterMock.Setup(x => x.TokensTransferSucceedAsync(FakePaymentId, It.IsAny<DateTime>()))
                .ReturnsAsync(PaymentStatusUpdateErrorCodes.PaymentIsInInvalidStatus)
                .Verifiable();

            var sut = CreateSutInstance();

            await sut.HandleAsync(_fakeTopics, FakeData, FakePartnersPaymentsContractAddress, DateTime.UtcNow);

            _paymentStatusUpdaterMock.Verify(
                x => x.TokensTransferSucceedAsync(FakePaymentId, It.IsAny<DateTime>()), Times.Once);
            _paymentTokensReservedPublisher.Verify(x => x.PublishAsync(It.IsAny<PartnersPaymentTokensReservedEvent>()),
                Times.Never);
        }

        [Fact]
        public async Task HandleAsync_EventIsPaymentReceived_TokensTransferSucceedAsyncCalled()
        {
            _settingsServiceMock.Setup(x => x.GetPartnersPaymentsAddress())
                .Returns(FakePartnersPaymentsContractAddress);

            _eventDecoderMock.Setup(x => x.GetEventType(_fakeTopics[0]))
                .Returns(BlockchainEventType.TransferReceived);

            _eventDecoderMock.Setup(x => x.DecodeTransferReceivedEvent(_fakeTopics, FakeData))
                .Returns(new PaymentProcessedByPartnerModel { PaymentRequestId = FakePaymentId });

            _paymentStatusUpdaterMock.Setup(x => x.TokensTransferSucceedAsync(FakePaymentId, It.IsAny<DateTime>()))
                .ReturnsAsync(PaymentStatusUpdateErrorCodes.None)
                .Verifiable();

            var sut = CreateSutInstance();

            await sut.HandleAsync(_fakeTopics, FakeData, FakePartnersPaymentsContractAddress, DateTime.UtcNow);

            _paymentStatusUpdaterMock.Verify(
                x => x.TokensTransferSucceedAsync(FakePaymentId, It.IsAny<DateTime>()), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_EventIsPaymentAccepted_ErrorWhenChangingTheStatus_PublisherNotCalled()
        {
            _settingsServiceMock.Setup(x => x.GetPartnersPaymentsAddress())
                .Returns(FakePartnersPaymentsContractAddress);

            _eventDecoderMock.Setup(x => x.GetEventType(_fakeTopics[0]))
                .Returns(BlockchainEventType.TransferAccepted);

            _eventDecoderMock.Setup(x => x.DecodeTransferAcceptedEvent(_fakeTopics, FakeData))
                .Returns(new PaymentProcessedByPartnerModel { PaymentRequestId = FakePaymentId });

            _paymentStatusUpdaterMock.Setup(x => x.TokensBurnSucceedAsync(FakePaymentId, It.IsAny<DateTime>()))
                .ReturnsAsync(PaymentStatusUpdateErrorCodes.PaymentIsInInvalidStatus)
                .Verifiable();


            var sut = CreateSutInstance();

            await sut.HandleAsync(_fakeTopics, FakeData, FakePartnersPaymentsContractAddress, DateTime.UtcNow);

            _paymentStatusUpdaterMock.Verify(
                x => x.TokensBurnSucceedAsync(FakePaymentId, It.IsAny<DateTime>()), Times.Once);
            _paymentProcessedPublisher.Verify(x => x.PublishAsync(It.IsAny<PartnersPaymentProcessedEvent>()),
                Times.Never);
        }


        [Fact]
        public async Task HandleAsync_EventIsPaymentAccepted_TokensBurnSucceedAsyncCalled()
        {
            _settingsServiceMock.Setup(x => x.GetPartnersPaymentsAddress())
                .Returns(FakePartnersPaymentsContractAddress);

            _eventDecoderMock.Setup(x => x.GetEventType(_fakeTopics[0]))
                .Returns(BlockchainEventType.TransferAccepted);

            _eventDecoderMock.Setup(x => x.DecodeTransferAcceptedEvent(_fakeTopics, FakeData))
                .Returns(new PaymentProcessedByPartnerModel { PaymentRequestId = FakePaymentId });

            _paymentStatusUpdaterMock.Setup(x => x.TokensBurnSucceedAsync(FakePaymentId, It.IsAny<DateTime>()))
                .ReturnsAsync(PaymentStatusUpdateErrorCodes.None)
                .Verifiable();

            _paymentsRepoMock.Setup(x => x.GetByPaymentRequestIdAsync(FakePaymentId))
                .ReturnsAsync(new PaymentModel
                {
                    TokensSendingAmount = FakeAmount
                });

            var sut = CreateSutInstance();

            await sut.HandleAsync(_fakeTopics, FakeData, FakePartnersPaymentsContractAddress, DateTime.UtcNow);

            _paymentProcessedPublisher.Verify(x => x.PublishAsync(It.IsAny<PartnersPaymentProcessedEvent>()),
                Times.Once);
        }

        [Fact]
        public async Task HandleAsync_EventIsPaymentRejected_ErrorWhenChangingTheStatus_PublisherNotCalled()
        {
            _settingsServiceMock.Setup(x => x.GetPartnersPaymentsAddress())
                .Returns(FakePartnersPaymentsContractAddress);

            _eventDecoderMock.Setup(x => x.GetEventType(_fakeTopics[0]))
                .Returns(BlockchainEventType.TransferRejected);

            _eventDecoderMock.Setup(x => x.DecodeTransferRejectedEvent(_fakeTopics, FakeData))
                .Returns(new PaymentProcessedByPartnerModel { PaymentRequestId = FakePaymentId });

            _paymentStatusUpdaterMock.Setup(x => x.TokensRefundSucceedAsync(FakePaymentId))
                .ReturnsAsync(PaymentStatusUpdateErrorCodes.PaymentIsInInvalidStatus)
                .Verifiable();


            var sut = CreateSutInstance();

            await sut.HandleAsync(_fakeTopics, FakeData, FakePartnersPaymentsContractAddress, DateTime.UtcNow);

            _paymentStatusUpdaterMock.Verify(
                x => x.TokensRefundSucceedAsync(FakePaymentId), Times.Once);
            _paymentProcessedPublisher.Verify(x => x.PublishAsync(It.IsAny<PartnersPaymentProcessedEvent>()),
                Times.Never);
        }

        [Fact]
        public async Task HandleAsync_EventIsPaymentRejected_TokensRefundSucceedAsyncCalled()
        {
            _settingsServiceMock.Setup(x => x.GetPartnersPaymentsAddress())
                .Returns(FakePartnersPaymentsContractAddress);

            _eventDecoderMock.Setup(x => x.GetEventType(_fakeTopics[0]))
                .Returns(BlockchainEventType.TransferRejected);

            _eventDecoderMock.Setup(x => x.DecodeTransferRejectedEvent(_fakeTopics, FakeData))
                .Returns(new PaymentProcessedByPartnerModel { PaymentRequestId = FakePaymentId });

            _paymentStatusUpdaterMock.Setup(x => x.TokensRefundSucceedAsync(FakePaymentId))
                .ReturnsAsync(PaymentStatusUpdateErrorCodes.None)
                .Verifiable();

            _paymentsRepoMock.Setup(x => x.GetByPaymentRequestIdAsync(FakePaymentId))
                .ReturnsAsync(new PaymentModel
                {
                    TokensSendingAmount = FakeAmount
                });

            var sut = CreateSutInstance();

            await sut.HandleAsync(_fakeTopics, FakeData, FakePartnersPaymentsContractAddress, DateTime.UtcNow);

            _paymentStatusUpdaterMock.Verify(x => x.TokensRefundSucceedAsync(FakePaymentId), Times.Once);
        }

        private UndecodedEventHandler CreateSutInstance()
        {
            return new UndecodedEventHandler(
                _paymentTokensReservedPublisher.Object,
                _paymentProcessedPublisher.Object,
                _paymentsRepoMock.Object,
                _eventDecoderMock.Object,
                _paymentStatusUpdaterMock.Object,
                _settingsServiceMock.Object,
                EmptyLogFactory.Instance);
        }
    }
}
