using System;
using JetBrains.Annotations;

namespace Lykke.Service.PartnersPayments.Contract
{
    /// <summary>
    /// Event which is raised when Payment request is created
    /// </summary>
    [PublicAPI]
    public class PartnerPaymentRequestCreatedEvent
    {
        /// <summary>
        /// Id of the request
        /// </summary>
        public string PaymentRequestId { get; set; }

        /// <summary>
        /// Id of the customer
        /// </summary>
        public string CustomerId { get; set; }

        /// <summary>
        /// Id of the partner who created the request
        /// </summary>
        public string PartnerId { get; set; }

        /// <summary>
        /// Timestamp when the request was created
        /// </summary>
        public DateTime Timestamp { get; set; }
    }
}
