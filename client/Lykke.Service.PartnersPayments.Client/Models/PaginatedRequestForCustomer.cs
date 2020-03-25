using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.PartnersPayments.Client.Models
{
    /// <summary>
    /// Request model which is used to request paginated data for a specific customer
    /// </summary>
    public class PaginatedRequestForCustomer : PaginatedRequestModel
    {
        /// <summary>
        /// Id of the customer
        /// </summary>
        [Required]
        public string CustomerId { get; set; }
    }
}
