using System;

namespace MAVN.Service.PartnersPayments.Domain.Enums
{
    public enum PaymentRequestStatus
    {
        Created,
        RejectedByCustomer,
        TokensTransferStarted,
        TokensTransferSucceeded,
        TokensTransferFailed,
        TokensBurnStarted,
        TokensRefundStarted,
        TokensBurnSucceeded,
        TokensBurnFailed,
        TokensRefundSucceeded,
        TokensRefundFailed,
        RequestExpired,
        ExpirationTokensRefundStarted,
        ExpirationTokensRefundSucceeded,
        ExpirationTokensRefundFailed,
        CancelledByPartner
    }
}
