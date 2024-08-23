using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
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
    public async Task<ApplicationResponse> AddApartmentPhoto(int apartmentId, CreateUpdateApartmentPhotoDTO createApartmentPhotoDTO)
    {
        _logger.LogInformation("Creating apartment photos to the Apartment with id: {ApartmentID}", apartmentId);
        var apartment = await _apartmentsRepository.GetByIdAsync(apartmentId) ??
                                throw new NotFoundException(nameof(Apartment), apartmentId.ToString());

        var photo = _mapper.Map<ApartmentPhoto>(createApartmentPhotoDTO);
        photo.ApartmentID = apartmentId;
        await _apartmentPhotosRepository.CreateAsync(photo);
        return new ApplicationResponse(true, StatusCodes.Status201Created, $"Photo added to the Apartment with id = {apartmentId}");
    }

    public async Task<ApplicationResponse> GetAllApartmentPhotos(int apartmentId)
    {
        _logger.LogInformation("Retrieving apartment photos for apartment with id: {ApartmentID}", apartmentId);
        var apartment = await _apartmentsRepository.GetByIdAsync(apartmentId)?? 
                                throw new NotFoundException(nameof(Apartment), apartmentId.ToString());

        var results = _mapper.Map<IEnumerable<ApartmentPhotoDTO>>(apartment.ApartmentPhotos);
        return new ApplicationResponse(true, StatusCodes.Status200OK, $"{results.Count()} records found", results);
    }

    public async Task<ApplicationResponse> GetByIdForApartment(int apartmentId, int id)
    {
        _logger.LogInformation("Retrieving apartment photo: {ApartmentPhotoId} for apartment with id: {ApartmentId}", id, apartmentId);
        var apartment = await _apartmentsRepository.GetByIdAsync(apartmentId) ??
                                throw new NotFoundException(nameof(Apartment), apartmentId.ToString());

        var apartmentPhoto = apartment.ApartmentPhotos.FirstOrDefault(x => x.ID == id) ??
                                throw new NotFoundException(nameof(ApartmentPhoto), id.ToString());

        var result = _mapper.Map<ApartmentPhotoDTO>(apartmentPhoto);
        return new ApplicationResponse(true, StatusCodes.Status200OK, $"OK", result);
    }

    public async Task<ApplicationResponse> UpdateApartmentPhoto(int apartmentId, ApartmentPhotoDTO updateApartmentPhotoDTO)
    {
        var currentUser = GetCurrentUser();

        _logger.LogInformation("Updating Apartment Photo with Id = {ApartmentPhotoId}", updateApartmentPhotoDTO.ID);
        var existingApartment = await _apartmentsRepository.GetByIdAsync(apartmentId) ??
                                throw new NotFoundException(nameof(Apartment), apartmentId.ToString());

        var apartmentPhoto = existingApartment.ApartmentPhotos.FirstOrDefault(x => x.ID == updateApartmentPhotoDTO.ID) ??
                                throw new NotFoundException(nameof(ApartmentPhoto), updateApartmentPhotoDTO.ID.ToString());

        var originalPhoto = new ApartmentPhoto() { Url = apartmentPhoto.Url };

        apartmentPhoto.Url = updateApartmentPhotoDTO.Url;

        var changeLogs = CoreUtilities.GenerateChangeLogs(originalPhoto, apartmentPhoto, currentUser.Email, apartmentPhoto.ID.ToString(), ["ApartmentID"]);

        await _changeLogsRepository.AddChangeLogs(changeLogs);

        await _apartmentPhotosRepository.SaveChangesAsync();

        var responseMessage = $"ApartmentPhoto with id = {updateApartmentPhotoDTO.ID}, for the Apartment with id = {apartmentId} is Updated";

        return new ApplicationResponse(true, StatusCodes.Status200OK, responseMessage, updateApartmentPhotoDTO);
    }

    public async Task<ApplicationResponse> DeleteApartmentPhoto(int apartmentId, int photoId)
    {
        var currentUser = GetCurrentUser();

        _logger.LogInformation("Deleting Apartment Photo with Id = {ApartmentPhotoId}", photoId);
        var existingApartment = await _apartmentsRepository.GetByIdAsync(apartmentId) ??
                                throw new NotFoundException(nameof(Apartment), apartmentId.ToString());

        var apartmentPhoto = existingApartment.ApartmentPhotos.FirstOrDefault(x => x.ID == photoId) ??
                                throw new NotFoundException(nameof(ApartmentPhoto), photoId.ToString());

        var originalPhoto = ApartmentPhoto.Clone(apartmentPhoto);

        apartmentPhoto.IsDeleted = true;

        var changeLogs = CoreUtilities.GenerateChangeLogs(originalPhoto, apartmentPhoto, currentUser.Email, apartmentPhoto.ID.ToString(), ["ApartmentID"]);

        await _changeLogsRepository.AddChangeLogs(changeLogs);

        await _apartmentPhotosRepository.SaveChangesAsync();

        return new ApplicationResponse(true, StatusCodes.Status200OK, "Apartment photo deleted!");
    }
    private CurrentUser GetCurrentUser()
    {
        var currentUser = _userContext.GetCurrentUser();
        if (currentUser == null)
        {
            _logger.LogWarning("User not authenticated.");
            throw new UnauthorizedAccessException("User not authenticated.");
        }
        return currentUser;
    }
}
