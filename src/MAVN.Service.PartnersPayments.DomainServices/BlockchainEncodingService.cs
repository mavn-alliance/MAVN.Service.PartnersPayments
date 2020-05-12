using MAVN.PrivateBlockchain.Definitions;
using MAVN.Service.PartnersPayments.Domain.Services;
using Nethereum.ABI;
using Nethereum.ABI.FunctionEncoding;
using Nethereum.Contracts;
using Nethereum.Hex.HexConvertors.Extensions;

namespace MAVN.Service.PartnersPayments.DomainServices
{
    public class BlockchainEncodingService : IBlockchainEncodingService
    {
        private readonly ABIEncode _abiEncode;
        private readonly FunctionCallEncoder _functionCallEncoder;
        public BlockchainEncodingService()
        {
            _functionCallEncoder = new FunctionCallEncoder();
            _abiEncode = new ABIEncode();
        }

        public string EncodePaymentRequestData(string partnerId, string locationId, long timestamp, string customerId, string transferId)
            => _abiEncode.GetABIEncoded(partnerId, locationId, timestamp, customerId, transferId).ToHex(true);

        public string EncodeAcceptRequestData(string partnerId, string locationId, long timestamp, string customerId, string transferId)
        {
            var func = new AcceptTransferFunction
            {
                PartnerId = partnerId,
                LocationId = locationId,
                Timestamp = timestamp,
                CustomerId = customerId,
                TransferId = transferId
            };

            return EncodeAcceptOrRejectRequestData(func);
        }

        public string EncodeRejectRequestData(string partnerId, string locationId, long timestamp, string customerId, string transferId)
        {
            var func = new RejectTransferFunction
            {
                PartnerId = partnerId,
                LocationId = locationId,
                Timestamp = timestamp,
                CustomerId = customerId,
                TransferId = transferId
            };

            return EncodeAcceptOrRejectRequestData(func);
        }

        private string EncodeAcceptOrRejectRequestData<T>(T func)
            where T : class, new()
        {
            var abiFunc = ABITypedRegistry.GetFunctionABI<T>();
            var result = _functionCallEncoder.EncodeRequest(func, abiFunc.Sha3Signature);

            return result;
        }
    }
}
