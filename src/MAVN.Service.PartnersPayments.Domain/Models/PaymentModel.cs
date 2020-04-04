using System;
using Falcon.Numerics;
using MAVN.Service.PartnersPayments.Domain.Enums;

namespace MAVN.Service.PartnersPayments.Domain.Models
{
    public class PaymentModel
    {
        public string PaymentRequestId { get; set; }

        public string CustomerId { get; set; }

        public string PartnerId { get; set; }

        public PaymentRequestStatus Status { get; set; }

        public string LocationId { get; set; }

        public string PosId { get; set; }

        public Money18 TokensAmount { get; set; }

        public Money18? TokensSendingAmount { get; set; }

        public decimal? FiatSendingAmount { get; set; }

        public decimal FiatAmount { get; set; }

        public decimal TotalBillAmount { get; set; }

        public string Currency { get; set; }

        public string PartnerMessageId { get; set; }

        public Money18 TokensToFiatConversionRate { get; set; }

        public DateTime? TokensReserveTimestamp { get; set; }

        public DateTime? TokensBurnTimestamp { get; set; }

        public DateTime Timestamp { get; set; }

        public DateTime LastUpdatedTimestamp { get; set; }

        public DateTime CustomerActionExpirationTimestamp { get; set; }

        public int CustomerActionExpirationTimeLeftInSeconds { get; set; }
    }
}
