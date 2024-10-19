using Apartments.Application.Common;
using Apartments.Application.Dtos.ApartmentPhotoDtos;
using Apartments.Application.IServices;
using Apartments.Domain.Common;
using Apartments.Domain.Entities;
using Apartments.Domain.Exceptions;
using Apartments.Domain.IRepositories;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Apartments.Application.Services;

public class ApartmentPhotoService(
    ILogger<ApartmentPhotoService> logger,
    IMapper mapper,
    IUserContext userContext,
    IAuthorizationManager authorizationManager,
    IAzureBlobStorageService azureBlobStorageService,
    IApartmentRepository apartmentRepository,
    IApartmentPhotoRepository apartmentPhotoRepository
) : IApartmentPhotoService
{
    public async Task<ServiceResult<IEnumerable<ApartmentPhotoDto>>> AddPhotosToApartment(
        UploadApartmentPhotoDto uploadApartmentPhotoDto)
    {
        if (!uploadApartmentPhotoDto.ApartmentPhotos.Any())
            return ServiceResult<IEnumerable<ApartmentPhotoDto>>.ErrorResult(StatusCodes.Status422UnprocessableEntity,
                "Please add at lease 1 image");

        var currentUser = userContext.GetCurrentUser();

        logger.LogInformation("Adding photos to apartment with Id = {Id}", uploadApartmentPhotoDto.ApartmentId);

        var apartment = await apartmentRepository.GetApartmentByIdAsync(uploadApartmentPhotoDto.ApartmentId) ??
                        throw new NotFoundException(nameof(Apartment), uploadApartmentPhotoDto.ApartmentId.ToString());

        if (!authorizationManager.AuthorizeApartmentPhoto(currentUser, ResourceOperation.Create, apartment.OwnerId))
            throw new ForbiddenException($"{currentUser.Email} not authorized to create an apartment photo");

        if (PhotosLimitReached(apartment))
        {
            var message =
                $"Apartment with ID {apartment.Id} already has {AppConstants.PhotosLimit} photos, please remove a photo before adding a new one";
            logger.LogInformation(message);
            return ServiceResult<IEnumerable<ApartmentPhotoDto>>.ErrorResult(StatusCodes.Status417ExpectationFailed,
                message);
        }

        await using var transaction = await apartmentPhotoRepository.BeginTransactionAsync();

        try
        {
            var apartmentPhotos = await azureBlobStorageService.UploadApartmentPhotos(uploadApartmentPhotoDto);

            var listApartmentPhotos = apartmentPhotos.ToList();
            if (listApartmentPhotos.Count == 0)
            {
                var message = $"No Photos were added to Apartment with ID {apartment.Id}";
                logger.LogInformation(message);
                return ServiceResult<IEnumerable<ApartmentPhotoDto>>.ErrorResult(StatusCodes.Status417ExpectationFailed,
                    message);
            }

            await apartmentPhotoRepository.AddApartmentPhotosAsync(listApartmentPhotos);

            // Commit the transaction
            await apartmentPhotoRepository.CommitTransactionAsync(transaction);

            // Map to the DTO to return as a response
            var listApartmentPhotoDto = mapper.Map<IEnumerable<ApartmentPhotoDto>>(apartmentPhotos);

            logger.LogInformation("Photos added to Apartment with ID {ApartmentId} successfully by {UserId}.",
                apartment.Id, currentUser.Email);
            return ServiceResult<IEnumerable<ApartmentPhotoDto>>.SuccessResult(listApartmentPhotoDto,
                "Apartment created successfully.");
        }
        catch (Exception ex)
        {
            // Rollback in case of any exceptions
            await transaction.RollbackAsync();
            logger.LogError(ex, "An error occurred while adding photos to the apartment. Transaction rolled back.");
            return ServiceResult<IEnumerable<ApartmentPhotoDto>>.ErrorResult(StatusCodes.Status500InternalServerError,
                "An error occurred while adding photos to the apartment");
        }
    }

    public async Task<ServiceResult<ApartmentPhotoDto>> GetApartmentPhotoById(int photoId, int apartmentId)
    {
        logger.LogInformation("Retrieving photo with Id = {PhotoId} from apartment with Id = {ApartmentId}", photoId,
            apartmentId);

        var apartment = await apartmentRepository.GetApartmentByIdAsync(apartmentId) ??
                        throw new NotFoundException(nameof(Apartment), apartmentId.ToString());

        var apartmentPhoto = apartment.ApartmentPhotos.FirstOrDefault(p => p.Id == photoId) ??
                             throw new NotFoundException(nameof(ApartmentPhoto), photoId.ToString());

        var apartmentPhotoDto = mapper.Map<ApartmentPhotoDto>(apartmentPhoto);

        return ServiceResult<ApartmentPhotoDto>.SuccessResult(apartmentPhotoDto);
    }

    public async Task<ServiceResult<IEnumerable<ApartmentPhotoDto>>> GetApartmentPhotos(int apartmentId)
    {
        logger.LogInformation("Retrieving photos from apartment with Id = {Id}", apartmentId);

        var apartment = await apartmentRepository.GetApartmentByIdAsync(apartmentId) ??
                        throw new NotFoundException(nameof(Apartment), apartmentId.ToString());

        var apartmentPhotos = mapper.Map<IEnumerable<ApartmentPhotoDto>>(apartment.ApartmentPhotos);

        return ServiceResult<IEnumerable<ApartmentPhotoDto>>.SuccessResult(apartmentPhotos);
    }

    public async Task<ServiceResult<string>> DeletePhotoFromApartment(int photoId, int apartmentId)
    {
        await using var transaction = await apartmentRepository.BeginTransactionAsync();
        try
        {
            var currentUser = userContext.GetCurrentUser();

            logger.LogInformation("Deleting photo with Id = {photoId} from apartment with Id = {apartmentId}", photoId,
                apartmentId);

            var apartment = await apartmentRepository.GetApartmentByIdAsync(apartmentId) ??
                            throw new NotFoundException(nameof(Apartment), apartmentId.ToString());

            if (!authorizationManager.AuthorizeApartmentPhoto(currentUser, ResourceOperation.Delete, apartment.OwnerId))
                throw new ForbiddenException($"{currentUser.Email} not authorized to delete this apartment photo");

            var apartmentPhoto = apartment.ApartmentPhotos.FirstOrDefault(x => x.Id == photoId) ??
                                 throw new NotFoundException(nameof(ApartmentPhoto), photoId.ToString());

            if(await azureBlobStorageService.DeleteAsync(apartmentPhoto.Url))
            {
                await apartmentPhotoRepository.DeleteApartmentPhotoAsync(apartmentPhoto);

                await transaction.CommitAsync();
                logger.LogInformation("Apartment photo deleted successfully.");
                return ServiceResult<string>.InfoResult(StatusCodes.Status200OK, "Apartment photo deleted successfully.");
            }

            await transaction.RollbackAsync();
            return ServiceResult<string>.ErrorResult(StatusCodes.Status500InternalServerError, "Apartment photo not deleted");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            await transaction.RollbackAsync();
            return ServiceResult<string>.ErrorResult(StatusCodes.Status500InternalServerError, "Photo not deleted");
        }
        
    }

    public async Task<ServiceResult<string>> RestoreApartmentPhoto(int photoId, int apartmentId)
    {
        var currentUser = userContext.GetCurrentUser();

        logger.LogInformation("Restoring photo with Id = {photoId} for apartment with Id = {apartmentId}", photoId,
            apartmentId);

        var apartment = await apartmentRepository.GetApartmentByIdAsync(apartmentId) ??
                        throw new NotFoundException(nameof(Apartment), apartmentId.ToString());

        if (!authorizationManager.AuthorizeApartmentPhoto(currentUser, ResourceOperation.Restore, apartment.OwnerId))
            throw new ForbiddenException($"{currentUser.Email} not authorized to restore this apartment photo");

        var apartmentPhoto = apartment.ApartmentPhotos.FirstOrDefault(x => x.Id == photoId) ??
                             throw new NotFoundException(nameof(ApartmentPhoto), photoId.ToString());

        await apartmentPhotoRepository.RestoreApartmentAsync(apartmentPhoto, currentUser.Email);

        var logMessage = $"Apartment photo with Id = {photoId} is restored successfully";
        logger.LogInformation(logMessage);

        return ServiceResult<string>.InfoResult(StatusCodes.Status200OK, logMessage);
    }

    private bool PhotosLimitReached(Apartment apartment)
    {
        var notDeletedPhotos = apartment.ApartmentPhotos
            .Count(x => !x.IsDeleted);

        return notDeletedPhotos >= AppConstants.PhotosLimit;
    }
}