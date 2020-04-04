using AutoMapper;
using Lykke.PrivateBlockchain.Definitions;
using MAVN.Service.PartnersPayments.Domain.Common;
using MAVN.Service.PartnersPayments.Domain.Enums;
using MAVN.Service.PartnersPayments.Domain.Models;
using Nethereum.ABI.FunctionEncoding;
using Nethereum.Contracts;

namespace MAVN.Service.PartnersPayments.DomainServices.Common
{
    public class BlockchainEventDecoder : IBlockchainEventDecoder
    {
        private readonly IMapper _mapper;
        private readonly EventTopicDecoder _eventTopicDecoder;
        private readonly string _transferAcceptedEventSignature;
        private readonly string _transferRejectedEventSignature;
        private readonly string _transferReceivedEventSignature;

        public BlockchainEventDecoder(IMapper mapper)
        {
            _eventTopicDecoder = new EventTopicDecoder();
            _transferAcceptedEventSignature = $"0x{ABITypedRegistry.GetEvent<TransferAcceptedEventDTO>().Sha3Signature}";
            _transferRejectedEventSignature = $"0x{ABITypedRegistry.GetEvent<TransferRejectedEventDTO>().Sha3Signature}";
            _transferReceivedEventSignature = $"0x{ABITypedRegistry.GetEvent<TransferReceivedEventDTO>().Sha3Signature}";
            _mapper = mapper;
        }

        public PaymentProcessedByPartnerModel DecodeTransferAcceptedEvent(string[] topics, string data)
        {
            var decodedEvent = DecodeEvent<TransferAcceptedEventDTO>(topics, data);

            return _mapper.Map<PaymentProcessedByPartnerModel>(decodedEvent);
        }

        public PaymentProcessedByPartnerModel DecodeTransferRejectedEvent(string[] topics, string data)
        {
            var decodedEvent = DecodeEvent<TransferRejectedEventDTO>(topics, data);

            return _mapper.Map<PaymentProcessedByPartnerModel>(decodedEvent);
        }

        public PaymentProcessedByPartnerModel DecodeTransferReceivedEvent(string[] topics, string data)
        {
            var decodedEvent = DecodeEvent<TransferReceivedEventDTO>(topics, data);

            return _mapper.Map<PaymentProcessedByPartnerModel>(decodedEvent);
        }

        public BlockchainEventType GetEventType(string topic)
        {
            if (topic == _transferReceivedEventSignature)
                return BlockchainEventType.TransferReceived;

            if (topic == _transferAcceptedEventSignature)
                return BlockchainEventType.TransferAccepted;

            if (topic == _transferRejectedEventSignature)
                return BlockchainEventType.TransferRejected;

            return BlockchainEventType.Unknown;
        }

        private T DecodeEvent<T>(string[] topics, string data) where T : class, new()
            => _eventTopicDecoder.DecodeTopics<T>(topics, data);
    }
}
