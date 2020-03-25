using Lykke.Service.PartnersPayments.Domain.Enums;
using Lykke.Service.PartnersPayments.Domain.Models;

namespace Lykke.Service.PartnersPayments.Domain.Common
{
    public interface IBlockchainEventDecoder
    {
        PaymentProcessedByPartnerModel DecodeTransferAcceptedEvent(string[] topics, string data);

        PaymentProcessedByPartnerModel DecodeTransferRejectedEvent(string[] topics, string data);

        PaymentProcessedByPartnerModel DecodeTransferReceivedEvent(string[] topics, string data);

        BlockchainEventType GetEventType(string topic);
    }
}
