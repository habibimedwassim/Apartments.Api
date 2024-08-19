using AutoMapper;
using RenTN.Application.DTOs.ApartmentPhotoDTOs;
using RenTN.Domain.Entities;

namespace RenTN.Application.Mapper;

public class ApartmentPhotoProfile : Profile
{
    public ApartmentPhotoProfile()
    {
        CreateMap<ApartmentPhoto, ApartmentPhotoDTO>().ReverseMap();
    }
}
