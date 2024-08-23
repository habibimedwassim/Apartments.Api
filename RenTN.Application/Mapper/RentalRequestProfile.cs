using AutoMapper;
using RenTN.Application.DTOs.RentalRequestDTOs;
using RenTN.Domain.Entities;

namespace RenTN.Application.Mapper;

public class RentalRequestProfile : Profile
{
    public RentalRequestProfile()
    {
        CreateMap<RentalRequest, RentalRequestDTO>()
            .ForMember(dest => dest.Tenant, opt => opt.MapFrom(src => new RentalRequestTenantDTO
            {
                ID = src.Tenant.SysID,
                UserName = src.Tenant.UserName!,
                FirstName = src.Tenant.FirstName,
                LastName = src.Tenant.LastName,
                Email = src.Tenant.Email!,
                PhoneNumber = src.Tenant.PhoneNumber
            }))
            .ForMember(dest => dest.Apartment, opt => opt.MapFrom(src => src.Apartment));
    }
}
