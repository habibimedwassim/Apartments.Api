using Apartments.Application.Dtos.RentTransactionDtos;
using Apartments.Domain.Entities;
using AutoMapper;

namespace Apartments.Application.AutoMapper;

public class RentTransactionMapper : Profile
{
    public RentTransactionMapper()
    {
        CreateMap<RentTransaction, RentTransaction>();
        CreateMap<RentTransaction, RentTransactionDto>()
            .ForMember(dest => dest.ApartmentOwner, opt => opt.MapFrom(src => src.Owner.Email));
    }
}
