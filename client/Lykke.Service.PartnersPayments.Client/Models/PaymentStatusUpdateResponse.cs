using Lykke.Service.PartnersPayments.Client.Enums;

namespace Lykke.Service.PartnersPayments.Client.Models
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
