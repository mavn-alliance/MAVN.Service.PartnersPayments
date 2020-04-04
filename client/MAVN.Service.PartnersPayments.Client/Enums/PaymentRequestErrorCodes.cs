using System;

namespace MAVN.Service.PartnersPayments.Client.Enums
{
    /// <summary>
    /// Error codes
    /// </summary>
    public enum PaymentRequestErrorCodes
    {
        /// <summary>
        /// No error
        /// </summary>
        None,
        /// <summary>
        /// Customer does not exist in the system
        /// </summary>
        CustomerDoesNotExist,
        /// <summary>
        /// Customer's wallet is blocked
        /// </summary>
        CustomerWalletBlocked,
        /// <summary>
        /// It is allowed to pass either Fiat or Tokens amount, not both
        /// </summary>
        CannotPassBothFiatAndTokensAmount,
        /// <summary>
        /// Fiat or Tokens amount should be passed
        /// </summary>
        EitherFiatOrTokensAmountShouldBePassed,
        /// <summary>
        /// Tokens amount should be a positive number
        /// </summary>
        InvalidTokensAmount,
        /// <summary>
        /// Fiat amount should be a positive number
        /// </summary>
        InvalidFiatAmount,
        /// <summary>
        /// Provided currency is not a valid one
        /// </summary>
        ///TODO:This error code is not used and should be removed later to avoid breaking changes now
        [Obsolete]
        InvalidCurrency,
        /// <summary>
        /// Total Bill amount should be a positive number
        /// </summary>
        InvalidTotalBillAmount,
        /// <summary>
        /// The provided PartnerId is not a valid Guid
        /// </summary>
        PartnerIdIsNotAValidGuid,
        /// <summary>
        /// Partner does not exist
        /// </summary>
        PartnerDoesNotExist,
        /// <summary>
        /// The provided locationId does not match any of the partner's
        /// </summary>
        NoSuchLocationForThisPartner,
        /// <summary>
        /// TokensRate or CurrencyRate in the used Partner is not valid
        /// </summary>
        InvalidTokensOrCurrencyRateInPartner,
        /// <summary>
        /// The provided CustomerId is not a valid Guid
        /// </summary>
        CustomerIdIsNotAValidGuid
    }
}
