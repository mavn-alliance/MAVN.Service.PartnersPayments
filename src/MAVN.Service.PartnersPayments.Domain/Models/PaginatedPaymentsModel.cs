using System.Collections.Generic;

namespace MAVN.Service.PartnersPayments.Domain.Models
{
    public class PaginatedPaymentsModel
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public IEnumerable<PaymentModel> PaymentRequests { get; set; }
    }
}
