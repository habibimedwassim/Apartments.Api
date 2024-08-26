using AutoMapper;
using RenTN.Application.DTOs.IdentityDTO;
using RenTN.Application.DTOs.IdentityDTOs;
using RenTN.Domain.Entities;

namespace RenTN.Application.Mapper;

public class IdentityProfile : Profile
{
    public IdentityProfile()
    {
        CreateMap<User, BaseProfileDTO>()
            .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.SysID));

        CreateMap<User, UserProfileDTO>()
            .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.SysID))
            .ForMember(dest => dest.CurrentApartment, opt => opt.NullSubstitute(null));

        CreateMap<User, OwnerProfileDTO>()
            .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.SysID))
            .ForMember(dest => dest.OwnedApartments, opt => opt.Ignore());
    }
}
