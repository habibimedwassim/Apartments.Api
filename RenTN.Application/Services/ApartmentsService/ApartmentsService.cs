using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RenTN.Application.DTOs.ApartmentDTOs;
using RenTN.Application.DTOs.ApartmentPhotoDTOs;
using RenTN.Application.Users;
using RenTN.Application.Utilities;
using RenTN.Domain.Entities;
using RenTN.Domain.Exceptions;
using RenTN.Domain.Interfaces;

namespace RenTN.Application.Services.ApartmentsService;

public class ApartmentsService(
    ILogger<ApartmentsService> _logger,
    IMapper _mapper,
    IApartmentsRepository _apartmentsRepository,
    IChangeLogsRepository _changeLogsRepository,
    IUserContext _userContext) : IApartmentsService
{
    public async Task<ApplicationResponse> GetApartments()
    {
        _logger.LogInformation("Retrieving all Apartments.");

        var apartments = await _apartmentsRepository.GetAllAsync();
        if (!apartments.Any())
        {
            _logger.LogWarning("No apartment records found.");
            return new ApplicationResponse(false, StatusCodes.Status404NotFound, "No apartment records found.");
        }

        var apartmentsDTO = _mapper.Map<IEnumerable<ApartmentDTO>>(apartments);
        return new ApplicationResponse(true, StatusCodes.Status200OK, $"{apartments.Count()} records found.", apartmentsDTO);
    }

    public async Task<ApplicationResponse> GetApartmentByID(int id)
    {
        _logger.LogInformation("Retrieving Apartment with ID = {ApartmentId}", id);

        var apartment = await _apartmentsRepository.GetByIdAsync(id);
        if (apartment == null)
        {
            _logger.LogWarning("Apartment with ID = {ApartmentId} doesn't exist.", id);
            return new ApplicationResponse(false, StatusCodes.Status404NotFound, "Apartment not found.");
        }

        var apartmentDTO = _mapper.Map<ApartmentDTO>(apartment);
        return new ApplicationResponse(true, StatusCodes.Status200OK, "Apartment found.", apartmentDTO);
    }

    public async Task<ApplicationResponse> CreateApartment(CreateApartmentDTO createApartmentDTO)
    {
        var currentUser = GetCurrentUser();

        _logger.LogInformation("Creating a new Apartment.");

        var apartment = _mapper.Map<Apartment>(createApartmentDTO);
        apartment.OwnerID = currentUser.Id;

        var createdApartment = await _apartmentsRepository.CreateAsync(apartment);
        if (createdApartment == null)
        {
            _logger.LogError("Failed to create Apartment.");
            return new ApplicationResponse(false, StatusCodes.Status500InternalServerError, "Failed to create Apartment.");
        }

        var createdApartmentDTO = _mapper.Map<CreateApartmentDTO>(createdApartment);
        return new ApplicationResponse(true, StatusCodes.Status201Created, "Apartment created successfully.", createdApartmentDTO);
    }

    public async Task<ApplicationResponse> UpdateApartment(UpdateApartmentDTO updateApartmentDTO)
    {
        var currentUser = GetCurrentUser();

        _logger.LogInformation("Updating Apartment with ID = {ApartmentId}", updateApartmentDTO.ID);

        var existingApartment = await _apartmentsRepository.GetByIdAsync(updateApartmentDTO.ID) ??
                                throw new NotFoundException(nameof(Apartment), updateApartmentDTO.ID.ToString());

        var originalApartment = Apartment.Clone(existingApartment);
        _mapper.Map(updateApartmentDTO, existingApartment);

        var changeLogs = CoreUtilities.GenerateChangeLogs(originalApartment, existingApartment, currentUser.Email, updateApartmentDTO.ID.ToString());
        await _changeLogsRepository.AddChangeLogs(changeLogs);
        await _apartmentsRepository.SaveChangesAsync();

        var updatedApartmentDTO = _mapper.Map<UpdateApartmentDTO>(existingApartment);
        return new ApplicationResponse(true, StatusCodes.Status200OK, "Apartment updated successfully.", updatedApartmentDTO);
    }

    public async Task<ApplicationResponse> DeleteApartment(int id)
    {
        var currentUser = GetCurrentUser();

        _logger.LogInformation("Deleting Apartment with ID = {ApartmentId}", id);

        var existingApartment = await _apartmentsRepository.GetByIdAsync(id) ??
                                throw new NotFoundException(nameof(Apartment), id.ToString());

        var originalApartment = Apartment.Clone(existingApartment);
        existingApartment.IsDeleted = true;

        var changeLogs = CoreUtilities.GenerateChangeLogs(originalApartment, existingApartment, currentUser.Email, id.ToString());
        await _changeLogsRepository.AddChangeLogs(changeLogs);
        await _apartmentsRepository.SaveChangesAsync();

        return new ApplicationResponse(true, StatusCodes.Status200OK, "Apartment deleted successfully.");
    }

    public async Task<ApplicationResponse> GetApartmentPhotos(int id)
    {
        _logger.LogInformation("Retrieving photos for Apartment with ID = {ApartmentId}", id);

        var apartment = await _apartmentsRepository.GetByIdAsync(id) ??
                                throw new NotFoundException(nameof(Apartment), id.ToString());

        var apartmentPhotoDTOs = _mapper.Map<List<ApartmentPhotoDTO>>(apartment.ApartmentPhotos);
        return new ApplicationResponse(true, StatusCodes.Status200OK, $"{apartmentPhotoDTOs.Count()} records found.", apartmentPhotoDTOs);
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