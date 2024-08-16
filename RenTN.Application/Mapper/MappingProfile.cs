using AutoMapper;
using RenTN.Application.DTOs.ApartmentDTOs;
using RenTN.Domain.Entities;

namespace RenTN.Application.Mapper;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Map from ApartmentPhoto to ApartmentPhotoDTO
        CreateMap<ApartmentPhoto, ApartmentPhotoDTO>();

        // Map from Apartment to ApartmentDTO
        CreateMap<Apartment, ApartmentDTO>()
            .ForMember(dest => dest.ApartmentPhotoUrls, opt => opt.MapFrom(src => src.ApartmentPhotos.Select(photo => photo.Url).ToList()));

        // Map from CreateApartmentDTO to Apartment
        CreateMap<CreateApartmentDTO, Apartment>()
            .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City))
            .ForMember(dest => dest.Street, opt => opt.MapFrom(src => src.Street))
            .ForMember(dest => dest.PostalCode, opt => opt.MapFrom(src => src.PostalCode))
            .ForMember(dest => dest.ApartmentPhotos, opt => opt.MapFrom(src => src.ApartmentPhotoUrls.Select(url => new ApartmentPhoto
            {
                Url = url
            }).ToList()));

        // Map from UpdateApartmentDTO to Apartment
        CreateMap<UpdateApartmentDTO, Apartment>()
            .ForMember(dest => dest.Description, opt => opt.Condition(src => !string.IsNullOrEmpty(src.Description)))
            .ForMember(dest => dest.Size, opt => opt.Condition(src => src.Size.HasValue))
            .ForMember(dest => dest.Price, opt => opt.Condition(src => src.Price.HasValue))
            .ForMember(dest => dest.IsAvailable, opt => opt.Condition(src => src.IsAvailable.HasValue))
            .ForMember(dest => dest.City, opt => opt.Condition(src => !string.IsNullOrEmpty(src.City)))
            .ForMember(dest => dest.Street, opt => opt.Condition(src => !string.IsNullOrEmpty(src.Street)))
            .ForMember(dest => dest.PostalCode, opt => opt.Condition(src => !string.IsNullOrEmpty(src.PostalCode)))
            .ForMember(dest => dest.ApartmentPhotos, opt => opt.Condition(src => src.ApartmentPhotoUrls != null && src.ApartmentPhotoUrls.Count > 0));
    }
}
