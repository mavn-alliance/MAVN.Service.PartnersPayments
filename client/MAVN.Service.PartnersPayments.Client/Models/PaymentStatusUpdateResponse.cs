using MAVN.Service.PartnersPayments.Client.Enums;

namespace MAVN.Service.PartnersPayments.Client.Models
{
    /// <summary>
    /// Response model for payment status update
    /// </summary>
    public class PaymentStatusUpdateResponse
    {
        /// <summary>
        /// Error code
        /// </summary>
        public PaymentStatusUpdateErrorCodes Error { get; set; }
    }
}
