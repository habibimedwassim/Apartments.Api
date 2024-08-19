using AutoMapper;
using RenTN.Application.DTOs.IdentityDTO;
using RenTN.Domain.Entities;

namespace RenTN.Application.Mapper;

public class IdentityProfile : Profile
{
    public IdentityProfile()
    {
        // Mapping for User to UserProfileDTO
        CreateMap<User, UserProfileDTO>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
            .ForMember(dest => dest.CurrentApartment, opt => opt.MapFrom(src => src.CurrentApartment))
            .ForMember(dest => dest.CurrentApartment, opt => opt.NullSubstitute(null)); // This handles the case when the user has no apartments.

        // Mapping for User to OwnerProfileDTO
        CreateMap<User, OwnerProfileDTO>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
            .ForMember(dest => dest.OwnedApartments, opt => opt.Ignore());  // Populate this manually if needed
    }
}
