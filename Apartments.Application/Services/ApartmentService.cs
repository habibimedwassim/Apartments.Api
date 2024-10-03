using Apartments.Application.Common;
using Apartments.Application.Dtos.ApartmentDtos;
using Apartments.Application.Dtos.ApartmentPhotoDtos;
using Apartments.Application.IServices;
using Apartments.Domain.Common;
using Apartments.Domain.Entities;
using Apartments.Domain.Exceptions;
using Apartments.Domain.IRepositories;
using Apartments.Domain.QueryFilters;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Apartments.Application.Services;

public class ApartmentService(
    ILogger<ApartmentService> logger,
    IMapper mapper,
    IUserContext userContext,
    IAuthorizationManager authorizationManager,
    IUserRepository userRepository,
    IAzureBlobStorageService azureBlobStorageService,
    IApartmentPhotoRepository apartmentPhotoRepository,
    IApartmentRepository apartmentRepository
) : IApartmentService
{
    public async Task<PagedResult<ApartmentDto>> GetAllApartments(ApartmentQueryFilter apartmentQueryFilter)
    {
        logger.LogInformation("Retrieving All Apartments");

        var pagedModel = await apartmentRepository.GetApartmentsPagedAsync(apartmentQueryFilter);

        var apartmentsDto = mapper.Map<IEnumerable<ApartmentDto>>(pagedModel.Data);

        var result =
            new PagedResult<ApartmentDto>(apartmentsDto, pagedModel.DataCount, apartmentQueryFilter.pageNumber);

        return result;
    }

    public async Task<ServiceResult<ApartmentDto>> GetApartmentById(int id)
    {
        logger.LogInformation("Retrieving Apartment with Id = {Id}", id);

        var apartment = await apartmentRepository.GetApartmentByIdAsync(id) ??
                        throw new NotFoundException(nameof(Apartment), id.ToString());

        var apartmentDto = mapper.Map<ApartmentDto>(apartment);

        return ServiceResult<ApartmentDto>.SuccessResult(apartmentDto);
    }

    public async Task<ServiceResult<ApartmentDto>> CreateApartment(CreateApartmentDto createApartmentDto)
    {
        var currentUser = userContext.GetCurrentUser();

        if (!authorizationManager.AuthorizeApartment(currentUser, ResourceOperation.Create))
            throw new ForbiddenException($"{currentUser.Email} not authorized to create an apartment");

        // Start a transaction at the service layer
        await using var transaction = await apartmentRepository.BeginTransactionAsync();

        try
        {
            var availableFrom = createApartmentDto.AvailableFrom.HasValue
                                ? createApartmentDto.AvailableFrom.Value
                                : DateOnly.FromDateTime(DateTime.UtcNow);

            // Map apartment details and save to the database
            var apartment = new Apartment(currentUser.Id)
            {
                Title = createApartmentDto.Title,
                Street = createApartmentDto.Street,
                City = createApartmentDto.City,
                PostalCode = createApartmentDto.PostalCode,
                Description = createApartmentDto.Description,
                Size = createApartmentDto.Size,
                RentAmount = createApartmentDto.RentAmount,
                AvailableFrom = availableFrom,
            };

            var createdApartment = await apartmentRepository.AddApartmentAsync(apartment);
            if (createdApartment == null)
            {
                var message = "An error occurred while attempting to create an apartment.";
                logger.LogWarning(message);
                return ServiceResult<ApartmentDto>.ErrorResult(StatusCodes.Status400BadRequest, message);
            }

            // Upload photos to Azure Blob Storage and save URLs in the database
            var photosToUpload = new UploadApartmentPhotoDto()
            {
                ApartmentId = apartment.Id,
                ApartmentPhotos = createApartmentDto.ApartmentPhotos
            };

            var apartmentPhotos = await azureBlobStorageService.UploadApartmentPhotos(photosToUpload);

            await apartmentPhotoRepository.AddApartmentPhotosAsync(apartmentPhotos);

            // Commit the transaction
            await transaction.CommitAsync();

            // Map to the DTO to return as a response
            var createdApartmentDto = mapper.Map<ApartmentDto>(createdApartment);
            createdApartmentDto.ApartmentPhotos = mapper.Map<List<ApartmentPhotoDto>>(apartmentPhotos);

            logger.LogInformation("Apartment with ID {ApartmentId} created successfully by user {UserId}.",
                createdApartment.Id, currentUser.Id);
            return ServiceResult<ApartmentDto>.SuccessResult(createdApartmentDto, "Apartment created successfully.");
        }
        catch (Exception ex)
        {
            // Rollback in case of any exceptions
            await transaction.RollbackAsync();
            logger.LogError(ex, "An error occurred while creating the apartment with photos. Transaction rolled back.");
            return ServiceResult<ApartmentDto>.ErrorResult(StatusCodes.Status500InternalServerError,
                "An error occurred while creating the apartment with photos.");
        }
    }

    public async Task<ServiceResult<ApartmentDto>> UpdateApartment(int id, UpdateApartmentDto updateApartmentDto)
    {
        var currentUser = userContext.GetCurrentUser();

        logger.LogInformation("Updating Apartment with Id = {Id}", id);

        var existingApartment = await apartmentRepository.GetApartmentByIdAsync(id) ??
                                throw new NotFoundException(nameof(Apartment), id.ToString());

        if (!authorizationManager.AuthorizeApartment(currentUser, ResourceOperation.Update, existingApartment))
            throw new ForbiddenException($"{currentUser.Email} not authorized to Update this Apartment");

        var originalApartment = mapper.Map<Apartment>(existingApartment);

        mapper.Map(updateApartmentDto, existingApartment);

        await apartmentRepository.UpdateApartmentAsync(originalApartment, existingApartment, currentUser.Email);

        var updatedApartmentDto = mapper.Map<ApartmentDto>(existingApartment);
        updatedApartmentDto.ApartmentPhotos = mapper.Map<List<ApartmentPhotoDto>>(existingApartment.ApartmentPhotos);

        return ServiceResult<ApartmentDto>.SuccessResult(updatedApartmentDto, "Apartment updated successfully.");
    }

    public async Task<ServiceResult<string>> DeleteApartment(int id)
    {
        var currentUser = userContext.GetCurrentUser();

        logger.LogInformation("Deleting Apartment with Id = {Id}", id);

        var apartment = await apartmentRepository.GetApartmentByIdAsync(id) ??
                        throw new NotFoundException(nameof(Apartment), id.ToString());

        if (!authorizationManager.AuthorizeApartment(currentUser, ResourceOperation.Delete, apartment))
            throw new ForbiddenException($"{currentUser.Email} not authorized to Delete this Apartment");

        await apartmentRepository.DeleteApartmentAsync(apartment, currentUser.Email);

        return ServiceResult<string>.InfoResult(StatusCodes.Status200OK, "Apartment deleted successfully.");
    }

    public async Task<ServiceResult<string>> RestoreApartment(int id)
    {
        var currentUser = userContext.GetCurrentUser();

        logger.LogInformation("Restoring Apartment with Id = {Id}", id);

        var apartment = await apartmentRepository.GetApartmentByIdAsync(id) ??
                        throw new NotFoundException(nameof(Apartment), id.ToString());

        if (!authorizationManager.AuthorizeApartment(currentUser, ResourceOperation.Restore, apartment))
            throw new ForbiddenException($"{currentUser.Email} not authorized to Restore this Apartment");

        await apartmentRepository.RestoreApartmentAsync(apartment, currentUser.Email);

        return ServiceResult<string>.InfoResult(StatusCodes.Status200OK, "Apartment restored successfully.");
    }

    public async Task<ServiceResult<PagedResult<ApartmentDto>>> GetOwnedApartments(ApartmentQueryFilter apartmentQueryFilter, int? ownerId = null)
    {
        var currentUser = userContext.GetCurrentUser();
        var sysId = ownerId ?? currentUser.SysId;

        logger.LogInformation("Retrieving All Apartments for Owner with Id = {Id}", ownerId);

        var user = await userRepository.GetBySysIdAsync(sysId) ??
                   throw new NotFoundException("User not found");

        if (user.Role == UserRoles.User)
            return ServiceResult<PagedResult<ApartmentDto>>.ErrorResult(StatusCodes.Status400BadRequest,
                "Only Owners have owned apartments");

        var pagedModel = await apartmentRepository.GetApartmentsPagedAsync(apartmentQueryFilter, user.Id);

        var apartmentsDto = mapper.Map<IEnumerable<ApartmentDto>>(pagedModel.Data);

        var pagedResult = new PagedResult<ApartmentDto>(apartmentsDto, pagedModel.DataCount, apartmentQueryFilter.pageNumber);

        var result = ServiceResult<PagedResult<ApartmentDto>>.SuccessResult(pagedResult);
            
        return result;
    }
}