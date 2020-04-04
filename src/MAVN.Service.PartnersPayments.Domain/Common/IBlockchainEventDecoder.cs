using MAVN.Service.PartnersPayments.Domain.Enums;
using MAVN.Service.PartnersPayments.Domain.Models;

namespace MAVN.Service.PartnersPayments.Domain.Common
{
    public interface IBlockchainEventDecoder
    {
        PaymentProcessedByPartnerModel DecodeTransferAcceptedEvent(string[] topics, string data);

        PaymentProcessedByPartnerModel DecodeTransferRejectedEvent(string[] topics, string data);

        PaymentProcessedByPartnerModel DecodeTransferReceivedEvent(string[] topics, string data);

        BlockchainEventType GetEventType(string topic);
    }
}
