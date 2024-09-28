using Apartments.Application.Dtos.ApartmentDtos;
using Apartments.Application.Dtos.ApartmentPhotoDtos;
using Apartments.Domain.Entities;
using AutoMapper;

namespace Apartments.Application.AutoMapper;

public class ApartmentMapper : Profile
{
    public ApartmentMapper()
    {
        // for cloning
        CreateMap<Apartment, Apartment>();
        CreateMap<ApartmentPhoto, ApartmentPhoto>();

        // mappers
        CreateMap<ApartmentPhoto, ApartmentPhotoDto>();

        CreateMap<Apartment, ApartmentDto>()
            .ForMember(dest => dest.ApartmentPhotos,
                       opt => opt.MapFrom(src => src.ApartmentPhotos.Select(photo => new ApartmentPhotoDto
                       {
                           Id = photo.Id,
                           CreatedDate = photo.CreatedDate,
                           Url = photo.Url
                       }).ToList()));

        CreateMap<CreateApartmentDto, Apartment>()
            .ForMember(dest => dest.ApartmentPhotos, opt => opt.Ignore())
            .ForMember(dest => dest.IsOccupied, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.OwnerId, opt => opt.Ignore());

        CreateMap<ApartmentDto, Apartment>()
            .ForMember(dest => dest.ApartmentPhotos, opt => opt.Ignore())
            .ForMember(dest => dest.IsOccupied, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.OwnerId, opt => opt.Ignore());

        CreateMap<UpdateApartmentDto, Apartment>()
            .ForMember(dest => dest.Description, opt => opt.Condition(src => src.Description != null))
            .ForMember(dest => dest.City, opt => opt.Condition(src => src.City != null))
            .ForMember(dest => dest.Street, opt => opt.Condition(src => src.Street != null))
            .ForMember(dest => dest.PostalCode, opt => opt.Condition(src => src.PostalCode != null))
            .ForMember(dest => dest.Size, opt => opt.Condition(src => src.Size.HasValue))
            .ForMember(dest => dest.RentAmount, opt => opt.Condition(src => src.RentAmount.HasValue))
            .ForMember(dest => dest.IsOccupied, opt => opt.Condition(src => src.IsOccupied.HasValue));
    }
}