using AutoMapper;
using Microsoft.Extensions.Logging;
using RenTN.Application.DTOs.ApartmentPhotoDTOs;
using RenTN.Domain.Entities;
using RenTN.Domain.Exceptions;
using RenTN.Domain.Interfaces;

namespace RenTN.Application.Services.ApartmentPhotosService;

public class ApartmentPhotosService(
    ILogger<ApartmentPhotosService> _logger,
    IMapper _mapper,
    IApartmentsRepository _apartmentsRepository) : IApartmentPhotosService
{
    public async Task<IEnumerable<ApartmentPhotoDTO>> GetAllApartmentPhotos(int apartmentId)
    {
        _logger.LogInformation("Retrieving apartment photos for apartment with id: {ApartmentID}", apartmentId);
        var apartment = await _apartmentsRepository.GetByIdAsync(apartmentId);
        if (apartment == null) throw new NotFoundException(nameof(Apartment), apartmentId.ToString());

        var results = _mapper.Map<IEnumerable<ApartmentPhotoDTO>>(apartment.ApartmentPhotos);
        return results;
    }

    public async Task<ApartmentPhotoDTO> GetByIdForApartment(int apartmentId, int id)
    {
        _logger.LogInformation("Retrieving apartment photo: {ApartmentPhotoId} for apartment with id: {ApartmentId}", id, apartmentId);
        var apartment = await _apartmentsRepository.GetByIdAsync(apartmentId);
        if (apartment == null) throw new NotFoundException(nameof(Apartment), apartmentId.ToString());

        var apartmentPhoto = apartment.ApartmentPhotos.FirstOrDefault(x => x.ID == id);
        if (apartmentPhoto == null) throw new NotFoundException(nameof(ApartmentPhoto), id.ToString());

        var result = _mapper.Map<ApartmentPhotoDTO>(apartmentPhoto);
        return result;
    }
}
