namespace MAVN.Service.PartnersPayments.Domain.Enums
{
    public enum PaymentRequestErrorCodes
    {
        None,
        CustomerDoesNotExist,
        CustomerWalletBlocked,
        CannotPassBothFiatAndTokensAmount,
        EitherFiatOrTokensAmountShouldBePassed,
        InvalidTokensAmount,
        InvalidFiatAmount,
        //TODO:This error code is not used and should be removed
        InvalidCurrency,
        InvalidTotalBillAmount,
        PartnerIdIsNotAValidGuid,
        PartnerDoesNotExist,
        NoSuchLocationForThisPartner,
        InvalidTokensOrCurrencyRateInPartner,
        CustomerIdIsNotAValidGuid
    }
}
