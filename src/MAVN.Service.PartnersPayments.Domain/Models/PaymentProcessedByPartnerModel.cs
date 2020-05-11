using MAVN.Numerics;

namespace MAVN.Service.PartnersPayments.Domain.Models
{
    public class PaymentProcessedByPartnerModel
    {
        public string PartnerId { get; set; }

        public string LocationId { get; set; }

        public long Timestamp { get; set; }

        public string CustomerId { get; set; }

        public string PaymentRequestId { get; set; }

        public Money18 Amount { get; set; }
    }
}
