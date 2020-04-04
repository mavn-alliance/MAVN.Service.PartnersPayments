using MAVN.Service.PartnersPayments.Client.Enums;

namespace MAVN.Service.PartnersPayments.Client.Models
{
    /// <summary>
    /// Response model for payment request
    /// </summary>
    public class PaymentRequestResponseModel
    {
        /// <summary>
        /// Id of the request
        /// </summary>
        public string PaymentRequestId { get; set; }

        /// <summary>
        /// Status of the request
        /// </summary>
        public PaymentRequestStatus? Status { get; set; }

        /// <summary>
        /// Error code
        /// </summary>
        public PaymentRequestErrorCodes Error { get; set; }
    }
}
