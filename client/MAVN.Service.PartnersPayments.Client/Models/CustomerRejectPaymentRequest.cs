using System.ComponentModel.DataAnnotations;

namespace MAVN.Service.PartnersPayments.Client.Models
{
    /// <summary>
    /// Request model for customer rejection of a payment request
    /// </summary>
    public class CustomerRejectPaymentRequest
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
    }
}
