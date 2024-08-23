using AutoMapper;
using RenTN.Application.DTOs.ApartmentDTOs;
using RenTN.Application.DTOs.ApartmentPhotoDTOs;
using RenTN.Domain.Entities;

namespace RenTN.Application.Mapper;

public class ApartmentProfile : Profile
{
    public ApartmentProfile()
    {
        // Map from ApartmentPhoto to ApartmentPhotoDTO
        CreateMap<ApartmentPhoto, ApartmentPhotoDTO>();

        // Map from Apartment to ApartmentDTO
        CreateMap<Apartment, ApartmentDTO>()
            .ForMember(dest => dest.ApartmentPhotos, opt => opt.MapFrom(src => src.ApartmentPhotos));

        // Map from CreateApartmentDTO to Apartment
        CreateMap<CreateApartmentDTO, Apartment>();

        // Map from UpdateApartmentDTO to Apartment
        CreateMap<UpdateApartmentDTO, Apartment>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // Map from Apartment to CreateApartmentDTO
        CreateMap<Apartment, CreateApartmentDTO>();

        // Map from Apartment to UpdateApartmentDTO
        CreateMap<Apartment, UpdateApartmentDTO>();
    }
}
