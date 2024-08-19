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
        CreateMap<Apartment, ApartmentDTO>();

        // Map from CreateApartmentDTO to Apartment
        CreateMap<CreateApartmentDTO, Apartment>();

        // Map from UpdateApartmentDTO to Apartment
        CreateMap<UpdateApartmentDTO, Apartment>();

        // Map from Apartment to CreateApartmentDTO
        CreateMap<Apartment, CreateApartmentDTO>();

        // Map from Apartment to UpdateApartmentDTO
        CreateMap<Apartment, UpdateApartmentDTO>();
    }
}
