using Apartments.Application.Dtos.UserDtos;
using Apartments.Domain.Entities;
using AutoMapper;

namespace Apartments.Application.AutoMapper;

public class UserMapper : Profile
{
    public UserMapper()
    {
        CreateMap<User, User>();

        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.SysId))
            .ForMember(dest => dest.CurrentApartment, opt => opt.NullSubstitute(null));

        CreateMap<UserDto, User>()
            .ForMember(dest => dest.SysId, opt => opt.MapFrom(src => src.Id));

        CreateMap<User, UserProfileDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.SysId))
            .ForMember(dest => dest.CurrentApartment, opt => opt.NullSubstitute(null))
            .ForMember(dest => dest.RentTransactions, opt => opt.Ignore())
            .ForMember(dest => dest.OwnedApartments, opt => opt.Ignore());
    }
}
