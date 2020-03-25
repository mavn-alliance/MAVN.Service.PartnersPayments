using System.ComponentModel.DataAnnotations;
using Falcon.Numerics;

namespace Lykke.Service.PartnersPayments.Client.Models
{
    /// <summary>
    /// Request model for customer approval of a payment request
    /// </summary>
    public class CustomerApprovePaymentRequest
    {
        /// <summary>
        /// Id of the payment request
        /// </summary>
        [Required]
        public string PaymentRequestId { get; set; }

        /// <summary>
        /// Id of the customer
        /// </summary>
        [Required]
        public string CustomerId { get; set; }

        /// <summary>
        /// Amount of tokens which customer is paying
        /// </summary>
        [Required]
        public Money18 SendingAmount { get; set; }
    }
}
