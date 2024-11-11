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
using Microsoft.IdentityModel.Tokens;

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

    public async Task<ServiceResult<string>> CreateApartment(CreateApartmentDto createApartmentDto)
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
                return ServiceResult<string>.ErrorResult(StatusCodes.Status400BadRequest, message);
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

            return ServiceResult<string>.InfoResult(StatusCodes.Status201Created, "Apartment created successfully.");
        }
        catch (Exception ex)
        {
            // Rollback in case of any exceptions
            await transaction.RollbackAsync();
            logger.LogError(ex, "An error occurred while creating the apartment with photos. Transaction rolled back.");
            return ServiceResult<string>.ErrorResult(StatusCodes.Status500InternalServerError,
                "An error occurred while creating the apartment with photos.");
        }
    }

    public async Task<ServiceResult<string>> UpdateApartment(int id, UpdateApartmentDto updateApartmentDto)
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

        return ServiceResult<string>.InfoResult(StatusCodes.Status200OK, "Apartment updated successfully.");
    }

    public async Task<ServiceResult<string>> DeleteApartment(int id, bool permanent)
    {
        try
        {
            var currentUser = userContext.GetCurrentUser();

            logger.LogInformation("Deleting Apartment with Id = {Id}", id);

            var apartment = await apartmentRepository.GetApartmentByIdAsync(id) ??
                            throw new NotFoundException(nameof(Apartment), id.ToString());

            if (!authorizationManager.AuthorizeApartment(currentUser, ResourceOperation.Delete, apartment))
                throw new ForbiddenException($"{currentUser.Email} not authorized to Archive/Restore this Apartment");

            if (permanent)
            {
                await apartmentRepository.DeleteApartmentPermanentlyAsync(apartment);
                return ServiceResult<string>.InfoResult(StatusCodes.Status200OK, "Apartment deleted permanently.");
            }

            await apartmentRepository.DeleteRestoreApartmentAsync(apartment, currentUser.Email);
            var message = apartment.IsDeleted ? "archived successfully" : "restored successfully";
            return ServiceResult<string>.InfoResult(StatusCodes.Status200OK, $"Apartment {message}.");
        }
        catch (NotFoundException ex)
        {
            logger.LogWarning(ex, "Apartment with Id = {Id} not found", id);
            return ServiceResult<string>.ErrorResult(StatusCodes.Status404NotFound, ex.Message);
        }
        catch (ForbiddenException ex)
        {
            logger.LogWarning(ex, ex.Message);
            return ServiceResult<string>.ErrorResult(StatusCodes.Status403Forbidden, ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while deleting apartment with Id = {Id}", id);
            return ServiceResult<string>.ErrorResult(StatusCodes.Status500InternalServerError, "An error occurred while deleting the apartment.");
        }
    }

    public async Task<ServiceResult<IEnumerable<ApartmentDto>>> GetOwnedApartments(int? ownerId = null)
    {
        var currentUser = userContext.GetCurrentUser();
        var sysId = ownerId ?? currentUser.SysId;

        logger.LogInformation("Retrieving All Apartments for Owner with Id = {Id}", sysId);

        var user = await userRepository.GetBySysIdAsync(sysId) ??
                   throw new NotFoundException("User not found");

        if (user.Role == UserRoles.User)
            return ServiceResult<IEnumerable<ApartmentDto>>.ErrorResult(StatusCodes.Status400BadRequest,
                "Only Owners have owned apartments");

        var apartments = await apartmentRepository.GetOwnedApartmentsAsync(user.Id);

        var apartmentsDto = mapper.Map<IEnumerable<ApartmentDto>>(apartments);

        return ServiceResult<IEnumerable<ApartmentDto>>.SuccessResult(apartmentsDto);
    }
    public async Task<PagedResult<ApartmentDto>> GetOwnedApartmentsPaged(int ownerId, int pageNumber)
    {
        var currentUser = userContext.GetCurrentUser();

        var apartmentQueryFilter = new ApartmentQueryFilter() {pageNumber = pageNumber };

        logger.LogInformation("Retrieving All Apartments");

        var user = await userRepository.GetBySysIdAsync(ownerId) ??
                  throw new NotFoundException("User not found");

        var pagedModel = await apartmentRepository.GetApartmentsPagedAsync(apartmentQueryFilter, user.Id);

        var apartmentsDto = mapper.Map<IEnumerable<ApartmentDto>>(pagedModel.Data);

        var result =
            new PagedResult<ApartmentDto>(apartmentsDto, pagedModel.DataCount, apartmentQueryFilter.pageNumber);

        return result;
    }
    public async Task<PagedResult<ApartmentDto>> GetOwnedApartmentsPaged(ApartmentQueryFilter apartmentQueryFilter)
    {
        var currentUser = userContext.GetCurrentUser();

        logger.LogInformation("Retrieving All Apartments");

        var pagedModel = await apartmentRepository.GetApartmentsPagedAsync(apartmentQueryFilter, currentUser.Id);

        var apartmentsDto = mapper.Map<IEnumerable<ApartmentDto>>(pagedModel.Data);

        var result =
            new PagedResult<ApartmentDto>(apartmentsDto, pagedModel.DataCount, apartmentQueryFilter.pageNumber);

        return result;
    }
    public async Task<IEnumerable<ApartmentDto>> GetBookmarkedApartments(List<int> apartmentsIds)
    {
        if (apartmentsIds.IsNullOrEmpty())
        {
            return [];
        }
        var apartments = await apartmentRepository.GetApartmentsList(apartmentsIds);

        var apartmentDtos = mapper.Map<IEnumerable<ApartmentDto>>(apartments);

        return apartmentDtos;
    }
}