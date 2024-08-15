using AutoMapper;
using RenTN.Application.DTOs.ApartmentDTOs;
using RenTN.Domain.Entities;

namespace RenTN.Application.Mapper;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Apartment, ApartmentDTO>();
        CreateMap<ApartmentPhoto, ApartmentPhotoDTO>();
        CreateMap<Location, LocationDTO>();

        CreateMap<CreateApartmentDTO, Apartment>()
            .ForMember(dest => dest.Location, opt => opt.MapFrom(src => new Location
            {
                City = src.City,
                Street = src.Street,
                PostalCode = src.PostalCode
            }))
            .ForMember(dest => dest.ApartmentPhotos, opt => opt.MapFrom(src => src.ApartmentPhotoUrls.Select(url => new ApartmentPhoto
            {
                Url = url
            }).ToList()));
    }
}
