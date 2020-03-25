using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.PartnersPayments.Client.Models
{
    /// <summary>
    /// Model which is used when requesting paginated data
    /// </summary>
    public class PaginatedRequestModel
    {
        /// <summary>
        /// The Current Page
        /// </summary>
        [Range(1, 10000)]
        public int CurrentPage { get; set; }

        /// <summary>
        /// The amount of items that the page holds
        /// </summary>
        [Range(1, 500)]
        public int PageSize { get; set; }
    }
}
