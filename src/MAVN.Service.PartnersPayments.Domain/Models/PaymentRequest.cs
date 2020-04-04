using System;
using Falcon.Numerics;

namespace MAVN.Service.PartnersPayments.Domain.Models
{
    public class PaymentRequest : IPaymentRequest
    {
        public string PaymentRequestId { get; set; }

        public string CustomerId { get; set; }

        public string PartnerId { get; set; }

        public string LocationId { get; set; }

        public string PosId { get; set; }

        public Money18? TokensAmount { get; set; }

        public decimal? FiatAmount { get; set; }

        public decimal TotalBillAmount { get; set; }

        public string Currency { get; set; }

        public string PartnerMessageId { get; set; }

        public Money18 TokensToFiatConversionRate { get; set; }

        public int? CustomerExpirationInSeconds { get; set; }

        public DateTime CustomerActionExpirationTimestamp { get; set; }
    }
}
