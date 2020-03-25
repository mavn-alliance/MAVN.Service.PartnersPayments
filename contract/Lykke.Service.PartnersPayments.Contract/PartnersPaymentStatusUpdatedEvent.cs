using JetBrains.Annotations;

namespace Lykke.Service.PartnersPayments.Contract
{
    /// <summary>
    /// The partner payment status update
    /// </summary>
    [PublicAPI]
    public class PartnersPaymentStatusUpdatedEvent
    {
        /// <summary>
        /// The payment request id
        /// </summary>
        public string PaymentRequestId { get; set; }
        
        /// <summary>
        /// The new status value
        /// </summary>
        public PartnerPaymentStatus Status { get; set; }
    }
}
