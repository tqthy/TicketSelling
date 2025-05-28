using AutoMapper;
using PaymentService.Api.DTOs;
using PaymentService.Core.Contracts.Gateways;

namespace PaymentService.Api.MappingProfiles;

public class PaymentMappingProfile : Profile
{
    public PaymentMappingProfile()
    {
        CreateMap<GetVnPayUrlDto, CreatePaymentRequest>()
            .ForMember(dest => dest.PaymentGateway, opt => opt.MapFrom(src => "VnPay"));
    }
}