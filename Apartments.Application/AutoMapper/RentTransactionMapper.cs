using Apartments.Application.Dtos.ApartmentDtos;
using Apartments.Application.Dtos.RentTransactionDtos;
using Apartments.Application.Dtos.UserDtos;
using Apartments.Application.Utilities;
using Apartments.Domain.Common;
using Apartments.Domain.Entities;
using AutoMapper;

namespace Apartments.Application.AutoMapper;

public class RentTransactionMapper : Profile
{
    public RentTransactionMapper()
    {
        CreateMap<RentTransaction, RentTransaction>();
        CreateMap<RentTransaction, RentTransactionDto>()
            .ForMember(dest => dest.Apartment, opt => opt.MapFrom(src => src.Apartment))
            .ForMember(dest => dest.TenantId, opt => opt.MapFrom(src => src.Tenant.SysId))
            .ForMember(dest => dest.Avatar,
                opt =>
                {
                    opt.MapFrom(src => src.Tenant.Avatar);
                    opt.Condition(src => src.Tenant != null && !string.IsNullOrEmpty(src.Tenant.Avatar));
                });

        CreateMap<Apartment, ApartmentPreviewModel>()
            .ForMember(dest => dest.Owner, opt => opt.MapFrom((src, dest, destMember, context) =>
            {
                // Check if the current user is an owner and if so, exclude the Owner property
                var currentUserRole = context.Items["UserRole"] as string;
                return currentUserRole == UserRoles.Owner ? null : context.Mapper.Map<OwnerDto>(src.Owner);
            }));

        CreateMap<User, OwnerDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => CoreUtilities.ConstructUserFullName(src.FirstName, src.LastName)));
    }
}