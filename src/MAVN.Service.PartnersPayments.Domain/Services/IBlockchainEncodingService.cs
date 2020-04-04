namespace MAVN.Service.PartnersPayments.Domain.Services
{
    public interface IBlockchainEncodingService
    {
        string EncodePaymentRequestData(
            string partnerId,
            string locationId,
            long timestamp,
            string customerId,
            string transferId);

        string EncodeAcceptRequestData(
            string partnerId,
            string locationId,
            long timestamp,
            string customerId,
            string transferId);

        string EncodeRejectRequestData(
            string partnerId,
            string locationId,
            long timestamp,
            string customerId,
            string transferId);
    }
}
