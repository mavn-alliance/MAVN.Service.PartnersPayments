namespace Lykke.Service.PartnersPayments.Domain.Models
{
    public interface IPaymentRequestBlockchainData
    {
        string PaymentRequestId { get; set; }

        string LastOperationId { get; set; }
    }
}
