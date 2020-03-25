namespace Lykke.Service.PartnersPayments.Domain.Enums
{
    public enum PaymentStatusUpdateErrorCodes
    {
        None,
        InvalidSenderId,
        InvalidRecipientId,
        SenderWalletMissing,
        RecipientWalletMissing,
        InvalidAmount,
        NotEnoughFunds,
        DuplicateRequest,
        InvalidAdditionalDataFormat,
        PaymentDoesNotExist,
        CustomerIdDoesNotMatch,
        PaymentIsInInvalidStatus,
        CustomerWalletIsBlocked,
        PartnerDoesNotExist,
        InvalidTokensOrCurrencyRateInPartner,
    }
}
