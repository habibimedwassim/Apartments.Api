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
            .ForMember(dest => dest.CurrentApartment, opt => opt.Ignore());

        CreateMap<UserDto, User>()
            .ForMember(dest => dest.SysId, opt => opt.MapFrom(src => src.Id));

        CreateMap<User, UserProfileDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.SysId))
            .ForMember(dest => dest.CurrentApartment, opt => opt.NullSubstitute(null))
            .ForMember(dest => dest.RentTransactions, opt => opt.Ignore())
            .ForMember(dest => dest.OwnedApartments, opt => opt.Ignore());

        CreateMap<UpdateUserDto, User>()
            .ForMember(dest => dest.FirstName, opt => opt.Condition(src => src.FirstName != null))
            .ForMember(dest => dest.LastName, opt => opt.Condition(src => src.LastName != null))
            .ForMember(dest => dest.PhoneNumber, opt => opt.Condition(src => src.PhoneNumber != null))
            .ForMember(dest => dest.Gender, opt => opt.Condition(src => src.Gender != null))
            .ForMember(dest => dest.DateOfBirth, opt => opt.Condition(src => src.DateOfBirth != null));
    }
}