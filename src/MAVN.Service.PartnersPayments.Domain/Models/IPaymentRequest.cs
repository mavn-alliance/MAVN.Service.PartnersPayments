using System;
using Falcon.Numerics;

namespace MAVN.Service.PartnersPayments.Domain.Models
{
    public interface IPaymentRequest
    {
        string PaymentRequestId { get; set; }

        string CustomerId { get; set; }

        string PartnerId { get; set; }

        string LocationId { get; set; }

        string PosId { get; set; }

        Money18? TokensAmount { get; set; }

        decimal? FiatAmount { get; set; }

        decimal TotalBillAmount { get; set; }

        string Currency { get; set; }

        string PartnerMessageId { get; set; }

        Money18 TokensToFiatConversionRate { get; set; }

        int? CustomerExpirationInSeconds { get; set; }

        DateTime CustomerActionExpirationTimestamp { get; set; }
    }
}
