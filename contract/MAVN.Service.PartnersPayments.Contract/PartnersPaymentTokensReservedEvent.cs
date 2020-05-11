using System;
using MAVN.Numerics;
using JetBrains.Annotations;

namespace MAVN.Service.PartnersPayments.Contract
{
    /// <summary>
    /// Event which notifies that tokens for a partner payment have been reserved
    /// </summary>
    [PublicAPI]
    public class PartnersPaymentTokensReservedEvent
    {
        /// <summary>
        /// Id of the payment request
        /// </summary>
        public string PaymentRequestId { get; set; }

        /// <summary>
        /// Id of the customer
        /// </summary>
        public string CustomerId { get; set; }

        /// <summary>
        /// Id of the partner
        /// </summary>
        public string PartnerId { get; set; }

        /// <summary>
        /// Id of the location
        /// </summary>
        public string LocationId { get; set; }

        /// <summary>
        /// Amount of tokens 
        /// </summary>
        public Money18 Amount { get; set; }

        /// <summary>
        /// Timestamp
        /// </summary>
        public DateTime Timestamp { get; set; }
    }
}
