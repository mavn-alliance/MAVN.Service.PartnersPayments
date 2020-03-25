using System;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.Service.PartnersPayments.Contract;
using Lykke.Service.PartnersPayments.Domain.Common;
using Lykke.Service.PartnersPayments.Domain.Enums;
using Lykke.Service.PartnersPayments.Domain.Models;
using Lykke.Service.PartnersPayments.Domain.RabbitMq.Handlers;
using Lykke.Service.PartnersPayments.Domain.Repositories;
using Lykke.Service.PartnersPayments.Domain.Services;

namespace Lykke.Service.PartnersPayments.DomainServices.RabbitMq.Handlers
{
    public class UndecodedEventHandler : IUndecodedEventHandler
    {
        private readonly IRabbitPublisher<PartnersPaymentTokensReservedEvent> _paymentTokensReservedPublisher;
        private readonly IRabbitPublisher<PartnersPaymentProcessedEvent> _paymentProcessedPublisher;
        private readonly IPaymentsRepository _paymentsRepository;
        private readonly IBlockchainEventDecoder _eventDecoder;
        private readonly IPaymentsStatusUpdater _paymentsStatusUpdater;
        private readonly ISettingsService _settingsService;
        private readonly ILog _log;

        public UndecodedEventHandler(
            IRabbitPublisher<PartnersPaymentTokensReservedEvent> paymentTokensReservedPublisher,
            IRabbitPublisher<PartnersPaymentProcessedEvent> paymentProcessedPublisher,
            IPaymentsRepository paymentsRepository,
            IBlockchainEventDecoder eventDecoder,
            IPaymentsStatusUpdater paymentsStatusUpdater,
            ISettingsService settingsService,
            ILogFactory logFactory)
        {
            _paymentTokensReservedPublisher = paymentTokensReservedPublisher;
            _paymentProcessedPublisher = paymentProcessedPublisher;
            _paymentsRepository = paymentsRepository;
            _eventDecoder = eventDecoder;
            _paymentsStatusUpdater = paymentsStatusUpdater;
            _settingsService = settingsService;
            _log = logFactory.CreateLog(this);
        }
        public async Task HandleAsync(string[] topics, string data, string contractAddress, DateTime timestamp)
        {
            //This means that the event is raised by another smart contract and we are not interested in it
            if (!string.Equals(contractAddress, _settingsService.GetPartnersPaymentsAddress()
                , StringComparison.InvariantCultureIgnoreCase))
            {
                _log.Info("The contract address differs from the expected one. Event handling will be stopped.",
                    context: new { expected = _settingsService.GetPartnersPaymentsAddress(), current = contractAddress });

                return;
            }

            var eventType = _eventDecoder.GetEventType(topics[0]);

            switch (eventType)
            {
                case BlockchainEventType.Unknown:
                    return;
                case BlockchainEventType.TransferReceived:
                    await HandlePaymentReceived(topics, data, timestamp);
                    break;
                case BlockchainEventType.TransferAccepted:
                    await HandlePaymentAccepted(topics, data, timestamp);
                    break;
                case BlockchainEventType.TransferRejected:
                    await HandlePaymentRejected(topics, data);
                    break;
                default: throw new InvalidOperationException("Unsupported blockchain event type");
            }
        }

        private async Task HandlePaymentReceived(string[] topics, string data, DateTime timestamp)
        {
            var receivedPaymentModel = _eventDecoder.DecodeTransferReceivedEvent(topics, data);

            var statusUpdateResult =
                await _paymentsStatusUpdater.TokensTransferSucceedAsync(receivedPaymentModel.PaymentRequestId, timestamp);

            if (statusUpdateResult != PaymentStatusUpdateErrorCodes.None)
            {
                _log.Error(message: "Could not mark payment as TransferSucceeded because of error",
                    context: new { Error = statusUpdateResult, Payment = receivedPaymentModel });
                return;
            }

            await _paymentTokensReservedPublisher.PublishAsync(new PartnersPaymentTokensReservedEvent
            {
                PaymentRequestId = receivedPaymentModel.PaymentRequestId,
                PartnerId = receivedPaymentModel.PartnerId,
                CustomerId = receivedPaymentModel.CustomerId,
                LocationId = receivedPaymentModel.LocationId,
                Amount = receivedPaymentModel.Amount,
                Timestamp = DateTime.UtcNow
            });
        }

        private async Task HandlePaymentAccepted(string[] topics, string data, DateTime timestamp)
        {
            var acceptedPaymentModel = _eventDecoder.DecodeTransferAcceptedEvent(topics, data);

            var statusUpdateResult =
                await _paymentsStatusUpdater.TokensBurnSucceedAsync(acceptedPaymentModel.PaymentRequestId, timestamp);

            if (statusUpdateResult != PaymentStatusUpdateErrorCodes.None)
            {
                _log.Error(message: "Could not mark payment as BurnSucceeded because of error",
                    context: new { Error = statusUpdateResult, Payment = acceptedPaymentModel });

                return;
            }

            var partnerPayment = await GetPaymentRequest(acceptedPaymentModel.PaymentRequestId);
            //We should have value for Sending amount at this stage for sure
            acceptedPaymentModel.Amount = partnerPayment.TokensSendingAmount.Value;

            await PublishPaymentProcessedEvent(acceptedPaymentModel, ProcessedPartnerPaymentStatus.Accepted);
        }

        private async Task HandlePaymentRejected(string[] topics, string data)
        {
            var rejectedPaymentModel = _eventDecoder.DecodeTransferRejectedEvent(topics, data);

            var statusUpdateResult =
                await _paymentsStatusUpdater.TokensRefundSucceedAsync(rejectedPaymentModel.PaymentRequestId);

            if (statusUpdateResult != PaymentStatusUpdateErrorCodes.None)
            {
                _log.Error(message: "Could not mark payment as RefundSucceeded/ExpirationTokensRefundSucceeded because of error",
                    context: new { Error = statusUpdateResult, Payment = rejectedPaymentModel });

                return;
            }

            var partnerPayment = await GetPaymentRequest(rejectedPaymentModel.PaymentRequestId);
            //We should have value for Sending amount at this stage for sure
            rejectedPaymentModel.Amount = partnerPayment.TokensSendingAmount.Value;

            await PublishPaymentProcessedEvent(rejectedPaymentModel, ProcessedPartnerPaymentStatus.Rejected);
        }

        private async Task PublishPaymentProcessedEvent(PaymentProcessedByPartnerModel paymentModel,
            ProcessedPartnerPaymentStatus status)
        {
            await _paymentProcessedPublisher.PublishAsync(new PartnersPaymentProcessedEvent
            {
                Status = status,
                PaymentRequestId = paymentModel.PaymentRequestId,
                PartnerId = paymentModel.PartnerId,
                CustomerId = paymentModel.CustomerId,
                LocationId = paymentModel.LocationId,
                Amount = paymentModel.Amount,
                Timestamp = DateTime.UtcNow,
            });
        }

        private async Task<PaymentModel> GetPaymentRequest(string paymentRequestId)
        {
            var partnerPayment = await _paymentsRepository.GetByPaymentRequestIdAsync(paymentRequestId);

            if (partnerPayment == null)
            {
                throw new InvalidOperationException(
                    $"Partner Payment with id: {paymentRequestId} which was just processed was not found in DB");
            }

            return partnerPayment;
        }
    }
}
