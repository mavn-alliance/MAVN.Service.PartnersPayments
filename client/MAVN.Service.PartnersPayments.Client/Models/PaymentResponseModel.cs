using System;
using MAVN.Numerics;
using MAVN.Service.PartnersPayments.Client.Enums;

namespace MAVN.Service.PartnersPayments.Client.Models
{
    /// <summary>
    /// Response model for pending payments
    /// </summary>
    public class PaymentResponseModel
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
        /// Timestamp when the request was created
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Timestamp of the last update of the request
        /// </summary>
        public DateTime LastUpdatedDate { get; set; }
    }
}
