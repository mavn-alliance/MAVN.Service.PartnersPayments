namespace Lykke.Service.PartnersPayments.Client.Enums
{
    /// <summary>
    /// Payment statuses
    /// </summary>
    public enum PaymentRequestStatus
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
        /// The payment request has been cancelled by partner
        /// </summary>
        CancelledByPartner,
    }
}
