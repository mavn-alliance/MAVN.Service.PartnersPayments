using AutoMapper;
using Falcon.Numerics;
using Lykke.PrivateBlockchain.Definitions;
using Lykke.Service.PartnersPayments.Client.Models;
using Lykke.Service.PartnersPayments.Domain.Models;

namespace Lykke.Service.PartnersPayments.MappingProfiles
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<PaymentRequestModel, PaymentRequest>()
                .ForMember(x => x.PaymentRequestId, opt => opt.Ignore())
                .ForMember(x => x.CustomerActionExpirationTimestamp, opt => opt.Ignore())
                .ForMember(x => x.TokensToFiatConversionRate, opt => opt.Ignore());
            CreateMap<PaymentRequestResult, PaymentRequestResponseModel>();
            CreateMap<TransferAcceptedEventDTO, PaymentProcessedByPartnerModel>()
                .ForMember(x => x.Amount, opt => opt.Ignore())
                .ForMember(x => x.PaymentRequestId, opt => opt.MapFrom(x => x.TransferId));
            CreateMap<TransferRejectedEventDTO, PaymentProcessedByPartnerModel>()
                .ForMember(x => x.Amount, opt => opt.Ignore())
                .ForMember(x => x.PaymentRequestId, opt => opt.MapFrom(x => x.TransferId));
            CreateMap<TransferReceivedEventDTO, PaymentProcessedByPartnerModel>()
                .ForMember(x => x.Amount, opt => opt.MapFrom(src => Money18.CreateFromAtto(src.Amount)))
                .ForMember(x => x.PaymentRequestId, opt => opt.MapFrom(x => x.TransferId));
            CreateMap<PaymentModel, PaymentResponseModel>()
                .ForMember(x => x.Date, opt => opt.MapFrom(x => x.Timestamp))
                .ForMember(x => x.LastUpdatedDate, opt => opt.MapFrom(x => x.LastUpdatedTimestamp));
            CreateMap<PaymentModel, PaymentDetailsResponseModel>();
            CreateMap<PaginatedPaymentsModel, PaginatedPaymentRequestsResponse>();
        }
    }
}
