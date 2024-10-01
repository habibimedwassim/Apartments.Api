using Apartments.Application.Dtos.ApartmentRequestDtos;
using Apartments.Domain.Entities;
using AutoMapper;

namespace Apartments.Application.AutoMapper;

public class ApartmentRequestMapper : Profile
{
    public ApartmentRequestMapper()
    {
        CreateMap<ApartmentRequest, ApartmentRequest>();
        CreateMap<ApartmentRequest, ApartmentRequestDto>()
            .ForMember(dest => dest.TenantId, opt => opt.MapFrom(src => src.Tenant.SysId));
            //.ForMember(dest => dest.TenantId, opt => opt.Ignore());

        CreateMap<UpdateApartmentRequestDto, ApartmentRequest>()
            .ForMember(dest => dest.Reason, opt => opt.Condition(src => src.Reason != null))
            .ForMember(dest => dest.RequestDate, opt => opt.Condition(src => src.RequestDate != null))
            .ForMember(dest => dest.Status, opt => opt.Condition(src => src.Status != null));

    }
}