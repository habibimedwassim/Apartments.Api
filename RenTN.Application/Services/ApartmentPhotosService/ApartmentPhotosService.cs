using AutoMapper;
using Microsoft.Extensions.Logging;
using RenTN.Application.DTOs.ApartmentPhotoDTOs;
using RenTN.Application.Users;
using RenTN.Application.Utilities;
using RenTN.Domain.Entities;
using RenTN.Domain.Exceptions;
using RenTN.Domain.Interfaces;

namespace RenTN.Application.Services.ApartmentPhotosService;

public class ApartmentPhotosService(
    ILogger<ApartmentPhotosService> _logger,
    IMapper _mapper,
    IApartmentsRepository _apartmentsRepository,
    IApartmentPhotosRepository _apartmentPhotosRepository,
    IChangeLogsRepository _changeLogsRepository,
    IUserContext _userContext) : IApartmentPhotosService
{
    public async Task AddApartmentPhoto(int apartmentId, CreateUpdateApartmentPhotoDTO createApartmentPhotoDTO)
    {
        _logger.LogInformation("Creating apartment photos to the Apartment with id: {ApartmentID}", apartmentId);
        var apartment = await _apartmentsRepository.GetByIdAsync(apartmentId);
        if (apartment == null) throw new NotFoundException(nameof(Apartment), apartmentId.ToString());

        var photo = _mapper.Map<ApartmentPhoto>(createApartmentPhotoDTO);
        photo.ApartmentID = apartmentId;
        await _apartmentPhotosRepository.CreateAsync(photo);
    }

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

    public async Task UpdateApartmentPhoto(int apartmentId, ApartmentPhotoDTO updateApartmentPhotoDTO)
    {
        var currentUser = _userContext.GetCurrentUser();
        if (currentUser == null)
        {
            _logger.LogWarning("User not found!");
            return;
        }

        _logger.LogInformation("Updating Apartment Photo with Id = {ApartmentPhotoId}", updateApartmentPhotoDTO.ID);
        var existingApartment = await _apartmentsRepository.GetByIdAsync(apartmentId)
                                      ?? throw new NotFoundException(nameof(Apartment), apartmentId.ToString());

        var apartmentPhoto = existingApartment.ApartmentPhotos.FirstOrDefault(x => x.ID == updateApartmentPhotoDTO.ID);
        if (apartmentPhoto == null) throw new NotFoundException(nameof(ApartmentPhoto), updateApartmentPhotoDTO.ID.ToString());

        var originalPhoto = new ApartmentPhoto() { Url = apartmentPhoto.Url };

        apartmentPhoto.Url = updateApartmentPhotoDTO.Url;

        var changeLogs = CoreUtilities.GenerateChangeLogs(originalPhoto, apartmentPhoto, currentUser.Email, apartmentPhoto.ID.ToString(), ["ApartmentID"]);

        await _changeLogsRepository.AddChangeLogs(changeLogs);

        await _apartmentPhotosRepository.SaveChangesAsync();
    }

    public async Task DeleteApartmentPhoto(int apartmentId, int photoId)
    {
        var currentUser = _userContext.GetCurrentUser();
        if (currentUser == null)
        {
            _logger.LogWarning("User not found!");
            return;
        }

        _logger.LogInformation("Deleting Apartment Photo with Id = {ApartmentPhotoId}", photoId);
        var existingApartment = await _apartmentsRepository.GetByIdAsync(apartmentId)
                                      ?? throw new NotFoundException(nameof(Apartment), apartmentId.ToString());

        var apartmentPhoto = existingApartment.ApartmentPhotos.FirstOrDefault(x => x.ID == photoId);
        if (apartmentPhoto == null) throw new NotFoundException(nameof(ApartmentPhoto), photoId.ToString());

        var originalPhoto = ApartmentPhoto.Clone(apartmentPhoto);

        apartmentPhoto.IsDeleted = true;

        var changeLogs = CoreUtilities.GenerateChangeLogs(originalPhoto, apartmentPhoto, currentUser.Email, apartmentPhoto.ID.ToString(), ["ApartmentID"]);

        await _changeLogsRepository.AddChangeLogs(changeLogs);

        await _apartmentPhotosRepository.SaveChangesAsync();
    }
}
