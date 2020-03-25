using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Falcon.Numerics;
using Lykke.Service.PartnersPayments.Domain.Enums;
using Lykke.Service.PartnersPayments.Domain.Models;

namespace Lykke.Service.PartnersPayments.MsSqlRepositories.Entities
{
    [Table("partners_payments")]
    public class PartnerPaymentEntity
    {
        [Key, Required]
        [Column("payment_request_id")]
        public string PaymentRequestId { get; set; }

        [Required]
        [Column("customer_id")]
        public string CustomerId { get; set; }

        [Required]
        [Column("partner_id")]
        public string PartnerId { get; set; }

        [Required]
        [Column("status")]
        public PaymentRequestStatus Status { get; set; }

        [Column("location_id")]
        public string LocationId { get; set; }

        [Column("pos_id")]
        public string PosId { get; set; }

        [Column("tokens_amount")]
        public Money18 TokensAmount { get; set; }

        [Column("tokens_amount_paid_by_customer")]
        public Money18? TokensAmountPaidByCustomer { get; set; }

        [Column("fiat_amount_paid_by_customer")]
        public decimal? FiatAmountPaidByCustomer { get; set; }

        [Column("fiat_amount")]
        public decimal FiatAmount { get; set; }

        [Column("total_bill_amount")]
        public decimal TotalBillAmount { get; set; }

        [Column("currency")]
        public string Currency { get; set; }

        [Column("partner_message_id")]
        public string PartnerMessageId { get; set; }

        [Required]
        [Column("tokens_to_fiat_conversion_rate")]
        public Money18 TokensToFiatConversionRate { get; set; }

        [Column("tokens_reserve_timestamp")]
        public DateTime? TokensReserveTimestamp { get; set; }

        [Column("tokens_burn_timestamp")]
        public DateTime? TokensBurnTimestamp { get; set; }

        [Column("timestamp")]
        public DateTime Timestamp { get; set; }

        [Column("last_updated_timestamp")]
        public DateTime LastUpdatedTimestamp { get; set; }

        [Column("customer_action_expiration_timestamp")]
        public DateTime CustomerActionExpirationTimestamp { get; set; }

        public static PartnerPaymentEntity Create(IPaymentRequest request)
        {
            var now = DateTime.UtcNow;
            return new PartnerPaymentEntity
            {
                CustomerId = request.CustomerId,
                //We shouldn't have a case where values are not populated when trying to save the entity
                FiatAmount = request.FiatAmount.Value,
                TokensAmount = request.TokensAmount.Value,
                PartnerId = request.PartnerId,
                Currency = request.Currency,
                LocationId = request.LocationId,
                PartnerMessageId = request.PartnerMessageId,
                TotalBillAmount = request.TotalBillAmount,
                PaymentRequestId = request.PaymentRequestId,
                TokensAmountPaidByCustomer = null,
                FiatAmountPaidByCustomer = null,
                Status = PaymentRequestStatus.Created,
                TokensReserveTimestamp = null,
                TokensBurnTimestamp = null,
                Timestamp = now,
                LastUpdatedTimestamp = now,
                PosId = request.PosId,
                TokensToFiatConversionRate = request.TokensToFiatConversionRate,
                CustomerActionExpirationTimestamp = request.CustomerActionExpirationTimestamp
            };
        }

        public static PartnerPaymentEntity Create(PaymentModel paymentModel)
        {
            return new PartnerPaymentEntity
            {
                CustomerId = paymentModel.CustomerId,
                FiatAmount = paymentModel.FiatAmount,
                TokensAmount = paymentModel.TokensAmount,
                PartnerId = paymentModel.PartnerId,
                Currency = paymentModel.Currency,
                LocationId = paymentModel.LocationId,
                PartnerMessageId = paymentModel.PartnerMessageId,
                TotalBillAmount = paymentModel.TotalBillAmount,
                PaymentRequestId = paymentModel.PaymentRequestId,
                TokensAmountPaidByCustomer = paymentModel.TokensSendingAmount,
                FiatAmountPaidByCustomer = paymentModel.FiatSendingAmount,
                Status = paymentModel.Status,
                TokensReserveTimestamp = paymentModel.TokensReserveTimestamp,
                TokensBurnTimestamp = paymentModel.TokensBurnTimestamp,
                Timestamp = paymentModel.Timestamp,
                PosId = paymentModel.PosId,
                TokensToFiatConversionRate = paymentModel.TokensToFiatConversionRate,
                LastUpdatedTimestamp = paymentModel.LastUpdatedTimestamp,
                CustomerActionExpirationTimestamp = paymentModel.CustomerActionExpirationTimestamp
            };
        }
    }
}
