using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.PartnersPayments.Domain.Enums;
using Lykke.Service.PartnersPayments.Domain.RabbitMq.Handlers;
using Lykke.Service.PartnersPayments.Domain.Repositories;
using Lykke.Service.PartnersPayments.Domain.Services;

namespace Lykke.Service.PartnersPayments.DomainServices.RabbitMq.Handlers
{
    public class TransactionFailedEventHandler : ITransactionFailedEventHandler
    {
        private readonly IPaymentRequestBlockchainRepository _paymentRequestBlockchainRepository;
        private readonly IPaymentsRepository _paymentsRepository;
        private readonly IPaymentsStatusUpdater _paymentsStatusUpdater;
        private readonly ILog _log;

        public TransactionFailedEventHandler(
            IPaymentRequestBlockchainRepository paymentRequestBlockchainRepository,
            IPaymentsRepository paymentsRepository,
            IPaymentsStatusUpdater paymentsStatusUpdater,
            ILogFactory logFactory)
        {
            _paymentRequestBlockchainRepository = paymentRequestBlockchainRepository;
            _paymentsRepository = paymentsRepository;
            _paymentsStatusUpdater = paymentsStatusUpdater;
            _log = logFactory.CreateLog(this);
        }

        public async Task HandleAsync(string operationId)
        {
            var requestBlockchainData = await _paymentRequestBlockchainRepository.GetByOperationIdAsync(operationId);

            if (requestBlockchainData == null)
                return;

            var paymentRequestId = requestBlockchainData.PaymentRequestId;

            var paymentRequest = await _paymentsRepository.GetByPaymentRequestIdAsync(paymentRequestId);

            switch (paymentRequest.Status)
            {
                case PaymentRequestStatus.TokensTransferStarted:
                    var transferFailResult =
                        await _paymentsStatusUpdater.TokensTransferFailAsync(paymentRequestId);
                    if (transferFailResult != PaymentStatusUpdateErrorCodes.None)
                        _log.Error(
                            message: "Could not change payment request status to TokensTransferFailed because of error",
                            context: new
                            {
                                PaymentRequestId = paymentRequestId,
                                Error = transferFailResult
                            });
                    break;
                case PaymentRequestStatus.TokensBurnStarted:
                    var burnFailResult =
                        await _paymentsStatusUpdater.TokensBurnFailAsync(paymentRequestId);
                    if (burnFailResult != PaymentStatusUpdateErrorCodes.None)
                        _log.Error(message: "Could not change payment request status to TokensBurnFailed because of error",
                        context: new
                        {
                            PaymentRequestId = paymentRequestId,
                            Error = burnFailResult
                        });
                    break;
                case PaymentRequestStatus.TokensRefundStarted:
                case PaymentRequestStatus.ExpirationTokensRefundStarted:
                    var refundFailResult =
                        await _paymentsStatusUpdater.TokensRefundFailAsync(paymentRequestId);
                    if (refundFailResult != PaymentStatusUpdateErrorCodes.None)
                        _log.Error(
                            message:
                            "Could not change payment request status to TokensRefundFailed/ExpirationTokensRefundFailed because of error",
                            context: new {PaymentRequestId = paymentRequestId, Error = refundFailResult});
                    break;
                default:
                    _log.Error(
                        message:
                        "Cannot change payment request status to failed because it is not in the appropriate status",
                        context: new
                        {
                            PaymentRequestId = paymentRequestId,
                            CurrentStatus = paymentRequest.Status
                        });
                    break;
            }
        }
    }
}
