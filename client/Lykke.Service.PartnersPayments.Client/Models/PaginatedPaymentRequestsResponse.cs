using System.Collections.Generic;

namespace Lykke.Service.PartnersPayments.Client.Models
{
    /// <summary>
    /// paginated payments response model
    /// </summary>
    public class PaginatedPaymentRequestsResponse
    {
        /// <summary>
        /// Current page number
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// The size of the page
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Total count of all records
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Collection of payment requests
        /// </summary>
        public IEnumerable<PaymentResponseModel> PaymentRequests { get; set; }
    }
}
