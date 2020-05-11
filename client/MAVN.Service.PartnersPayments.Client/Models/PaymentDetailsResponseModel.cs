using System;
using MAVN.Numerics;
using MAVN.Service.PartnersPayments.Client.Enums;

namespace MAVN.Service.PartnersPayments.Client.Models
{
    /// <summary>
    /// Model which holds the details of a payment request
    /// </summary>
    public class PaymentDetailsResponseModel
    {
        /// <summary>
        /// Id of the request
        /// </summary>
        public string PaymentRequestId { get; set; }

        /// <summary>
        /// Id of the customer
        /// </summary>
        public string CustomerId { get; set; }

        /// <summary>
        /// Id of the partner
        /// </summary>
        public string PartnerId { get; set; }

        /// <summary>
        /// Status of the request
        /// </summary>
        public PaymentRequestStatus Status { get; set; }

        /// <summary>
        /// Id of the location
        /// </summary>
        public string LocationId { get; set; }

        /// <summary>
        /// Amount in tokens
        /// </summary>
        public Money18 TokensAmount { get; set; }

        /// <summary>
        /// Amount of tokens customer is willing to pay
        /// </summary>
        public Money18? TokensSendingAmount { get; set; }

        /// <summary>
        /// Amount of tokens customer is willing to pay converted into fiat
        /// </summary>
        public decimal? FiatSendingAmount { get; set; }

        /// <summary>
        /// Amount in Fiat
        /// </summary>
        public decimal FiatAmount { get; set; }

        /// <summary>
        /// Amount of the total bill
        /// </summary>
        public decimal TotalBillAmount { get; set; }

        /// <summary>
        /// Currency which is used
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Partner message id
        /// </summary>
        public string PartnerMessageId { get; set; }

        /// <summary>
        /// Timestamp of the tokens reservation
        /// </summary>
        public DateTime? TokensReserveTimestamp { get; set; }

        /// <summary>
        /// Timestamp when tokens were burn
        /// </summary>
        public DateTime? TokensBurnTimestamp { get; set; }

        /// <summary>
        /// Timestamp of the payment requests
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Timestamp of the last update of the request
        /// </summary>
        public DateTime LastUpdatedTimestamp { get; set; }

        /// <summary>
        /// Conversion rate used to convert tokens to fiat
        /// </summary>
        public decimal TokensToFiatConversionRate { get; set; }

        /// <summary>
        /// Time left before marking the request as expired if it is still in created state
        /// </summary>
        public int CustomerActionExpirationTimeLeftInSeconds { get; set; }

        /// <summary>
        /// Date when the request will expire if it is not approved/rejected by the customer
        /// </summary>
        public DateTime CustomerActionExpirationTimestamp { get; set; }
    }
}
