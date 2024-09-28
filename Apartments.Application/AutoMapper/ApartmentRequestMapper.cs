using Apartments.Application.Dtos.ApartmentRequestDtos;
using Apartments.Domain.Entities;
using AutoMapper;

namespace Apartments.Application.AutoMapper;

public class ApartmentRequestMapper : Profile
{
    public ApartmentRequestMapper()
    {
        CreateMap<ApartmentRequest, ApartmentRequest>();
        CreateMap<ApartmentRequest, ApartmentRequestDto>();
    }
}
