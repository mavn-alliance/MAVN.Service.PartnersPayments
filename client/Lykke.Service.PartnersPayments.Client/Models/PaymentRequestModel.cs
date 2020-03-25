using System;
using System.ComponentModel.DataAnnotations;
using Falcon.Numerics;

namespace Lykke.Service.PartnersPayments.Client.Models
{
    /// <summary>
    /// Request model for initiating a partner payment request
    /// </summary>
    public class PaymentRequestModel
    {
        /// <summary>
        /// Id of the customer
        /// </summary>
        [Required]
        public string CustomerId { get; set; }

        /// <summary>
        /// Id of the partner
        /// </summary>
        [Required]
        public string PartnerId { get; set; }

        /// <summary>
        /// Id of the location
        /// </summary>
        public string LocationId { get; set; }

        /// <summary>
        /// Id of the Pos
        /// </summary>
        public string PosId { get; set; }

        /// <summary>
        /// Desired amount of tokens to be paid
        /// </summary>
        public Money18? TokensAmount { get; set; }

        /// <summary>
        /// Desired amount of fiat to be paid
        /// </summary>
        public decimal? FiatAmount { get; set; }

        /// <summary>
        /// The total of the bill
        /// </summary>
        [Required]
        public decimal TotalBillAmount { get; set; }

        /// <summary>
        /// Fiat currency
        /// </summary>
        [Required]
        public string Currency { get; set; }

        /// <summary>
        /// The partner message id
        /// </summary>
        public string PartnerMessageId { get; set; }

        /// <summary>
        /// Expiration period in which the customer should change the status of the request
        /// </summary>
        [Range(1, int.MaxValue)] 
        public int? CustomerExpirationInSeconds { get; set; }
    }
}
