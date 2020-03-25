using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.PartnersPayments.Client.Models
{
    /// <summary>
    /// Request model which is used when a receptionist wants to approve or reject a payment
    /// </summary>
    public class ReceptionistProcessPaymentRequest
    {
        /// <summary>
        /// Id of the payment request
        /// </summary>
        [Required]
        public string PaymentRequestId { get; set; }
    }
}
