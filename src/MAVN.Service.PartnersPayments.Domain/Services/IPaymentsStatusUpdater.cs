using System;
using System.Threading.Tasks;
using MAVN.Numerics;
using MAVN.Service.PartnersPayments.Domain.Enums;

namespace MAVN.Service.PartnersPayments.Domain.Services
{
    public interface IPaymentsStatusUpdater
    {
        Task<PaymentStatusUpdateErrorCodes> ApproveByCustomerAsync(string paymentRequestId,
            Money18 sendingAmount, string customerId);

        Task<PaymentStatusUpdateErrorCodes> RejectByCustomerAsync(string paymentRequestId,
            string customerId);

        Task<PaymentStatusUpdateErrorCodes> TokensTransferSucceedAsync(string paymentRequestId, DateTime timestamp);

        Task<PaymentStatusUpdateErrorCodes> TokensTransferFailAsync(string paymentRequestId);

        Task<PaymentStatusUpdateErrorCodes> TokensBurnSucceedAsync(string paymentRequestId, DateTime timestamp);

        Task<PaymentStatusUpdateErrorCodes> TokensBurnFailAsync(string paymentRequestId);

        Task<PaymentStatusUpdateErrorCodes> ApproveByReceptionistAsync(string paymentRequestId);

        Task<PaymentStatusUpdateErrorCodes> CancelByPartnerAsync(string paymentRequestId);

        Task<PaymentStatusUpdateErrorCodes> TokensRefundSucceedAsync(string paymentRequestId);

        Task<PaymentStatusUpdateErrorCodes> TokensRefundFailAsync(string paymentRequestId);

        Task<PaymentStatusUpdateErrorCodes> StartExpireRefundAsync(string paymentRequestId);

        Task<PaymentStatusUpdateErrorCodes> ExpireRequestAsync(string paymentRequestId);
    }
}
