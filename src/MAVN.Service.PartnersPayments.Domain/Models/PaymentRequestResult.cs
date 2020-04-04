using MAVN.Service.PartnersPayments.Domain.Enums;

namespace MAVN.Service.PartnersPayments.Domain.Models
{
    public class PaymentRequestResult
    {
        public PaymentRequestErrorCodes Error { get; private set; }

        public PaymentRequestStatus? Status { get; private set; }

        public string PaymentRequestId { get; set; }

        public static PaymentRequestResult Succeeded(PaymentRequestStatus status, string paymentRequestId)
        {
            return new PaymentRequestResult
            {
                Status = status,
                PaymentRequestId = paymentRequestId
            };
        }

        public static PaymentRequestResult Failed(PaymentRequestErrorCodes error)
        {
            return new PaymentRequestResult
            {
                Error = error
            };
        }
    }
}
