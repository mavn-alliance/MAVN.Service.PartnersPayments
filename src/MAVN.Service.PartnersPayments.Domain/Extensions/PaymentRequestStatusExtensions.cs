using System;
using MAVN.Service.PartnersPayments.Contract;
using MAVN.Service.PartnersPayments.Domain.Enums;

namespace MAVN.Service.PartnersPayments.Domain.Extensions
{
    public static class PaymentRequestStatusExtensions
    {
        public static PartnerPaymentStatus ToContractModel(this PaymentRequestStatus src)
        {
            switch (src)
            {
                case PaymentRequestStatus.Created:
                    return PartnerPaymentStatus.Created;
                case PaymentRequestStatus.RequestExpired:
                    return PartnerPaymentStatus.RequestExpired;
                case PaymentRequestStatus.RejectedByCustomer:
                    return PartnerPaymentStatus.RejectedByCustomer;
                case PaymentRequestStatus.TokensBurnFailed:
                    return PartnerPaymentStatus.TokensBurnFailed;
                case PaymentRequestStatus.TokensBurnStarted:
                    return PartnerPaymentStatus.TokensBurnStarted;
                case PaymentRequestStatus.TokensBurnSucceeded:
                    return PartnerPaymentStatus.TokensBurnSucceeded;
                case PaymentRequestStatus.TokensRefundFailed:
                    return PartnerPaymentStatus.TokensRefundFailed;
                case PaymentRequestStatus.TokensRefundStarted:
                    return PartnerPaymentStatus.TokensRefundStarted;
                case PaymentRequestStatus.TokensRefundSucceeded:
                    return PartnerPaymentStatus.TokensRefundSucceeded;
                case PaymentRequestStatus.TokensTransferFailed:
                    return PartnerPaymentStatus.TokensTransferFailed;
                case PaymentRequestStatus.TokensTransferStarted:
                    return PartnerPaymentStatus.TokensTransferStarted;
                case PaymentRequestStatus.TokensTransferSucceeded:
                    return PartnerPaymentStatus.TokensTransferSucceeded;
                case PaymentRequestStatus.ExpirationTokensRefundFailed:
                    return PartnerPaymentStatus.ExpirationTokensRefundFailed;
                case PaymentRequestStatus.ExpirationTokensRefundStarted:
                    return PartnerPaymentStatus.ExpirationTokensRefundStarted;
                case PaymentRequestStatus.ExpirationTokensRefundSucceeded:
                    return PartnerPaymentStatus.ExpirationTokensRefundSucceeded;
                case PaymentRequestStatus.CancelledByPartner:
                    return PartnerPaymentStatus.CancelledByPartner;
                default:
                    throw new ArgumentOutOfRangeException(nameof(src), $"Payment request status value was not expected: {src.ToString()}");
            }
        }
    }
}
