using JetBrains.Annotations;

namespace MAVN.Service.PartnersPayments.Contract
{
    /// <summary>
    /// The partner payment statuses
    /// </summary>
    [PublicAPI]
    public enum PartnerPaymentStatus
    {
        /// <summary>
        /// Payment is created by receptionist
        /// </summary>
        Created,
        /// <summary>
        /// Payment is rejected by the customer
        /// </summary>
        RejectedByCustomer,
        /// <summary>
        /// Tokens are being transferred
        /// </summary>
        TokensTransferStarted,
        /// <summary>
        /// Tokens transfer is successful
        /// </summary>
        TokensTransferSucceeded,
        /// <summary>
        /// Tokens transfer failed
        /// </summary>
        TokensTransferFailed,
        /// <summary>
        /// Tokens are being burned
        /// </summary>
        TokensBurnStarted,
        /// <summary>
        /// Tokens are being refunded
        /// </summary>
        TokensRefundStarted,
        /// <summary>
        /// Tokens were successfully burned
        /// </summary>
        TokensBurnSucceeded,
        /// <summary>
        /// Tokens burn failed
        /// </summary>
        TokensBurnFailed,
        /// <summary>
        /// Tokens were successfully refunded
        /// </summary>
        TokensRefundSucceeded,
        /// <summary>
        /// Tokens refund failed
        /// </summary>
        TokensRefundFailed,
        /// <summary>
        /// The request has expired
        /// </summary>
        RequestExpired,
        /// <summary>
        /// Tokens refund started because the request has expired
        /// </summary>
        ExpirationTokensRefundStarted,
        /// <summary>
        /// Expiration tokens refund succeeded
        /// </summary>
        ExpirationTokensRefundSucceeded,
        /// <summary>
        /// Expiration tokens refund failed
        /// </summary>
        ExpirationTokensRefundFailed,
        /// <summary>
        /// The payment is cancelled by partner
        /// </summary>
        CancelledByPartner,
    }
}
