using AutoMapper;
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
    public async Task<IEnumerable<ApartmentDTO>> GetApartments()
    {
        _logger.LogInformation("Getting all Apartments");
        var apartments = await _apartmentsRepository.GetAllAsync();
        var apartmentsDTO = _mapper.Map<IEnumerable<ApartmentDTO>>(apartments);

        return apartmentsDTO;
    }

    public async Task<ApartmentDTO?> GetApartmentByID(int id)
    {
        _logger.LogInformation($"Getting Apartment with ID = {id}");
        var apartment = await _apartmentsRepository.GetByIdAsync(id)
                              ?? throw new NotFoundException(nameof(Apartment), id.ToString());

        var apartmentDTO = _mapper.Map<ApartmentDTO?>(apartment);
        return apartmentDTO;
    }

    public async Task<CreateApartmentDTO?> CreateApartment(CreateApartmentDTO createApartmentDTO)
    {
        var currentUser = _userContext.GetCurrentUser();
        if (currentUser == null)
        {
            _logger.LogWarning("User not found!");
            return null;
        };

        _logger.LogInformation("Creating a new Apartment");

        var apartment = _mapper.Map<Apartment>(createApartmentDTO);
        apartment.OwnerID = currentUser.Id;

        var createdApartment = await _apartmentsRepository.CreateAsync(apartment);
        var createdApartmentDTO = _mapper.Map<CreateApartmentDTO>(createdApartment);
        return createdApartmentDTO;
    }

    public async Task<UpdateApartmentDTO?> UpdateApartment(UpdateApartmentDTO updateApartmentDTO)
    {
        var currentUser = _userContext.GetCurrentUser();
        if (currentUser == null)
        {
            _logger.LogWarning("User not found!");
            return null;
        }

        _logger.LogInformation("Updating Apartment with Id = {ApartmentId}", updateApartmentDTO.ID);

        var existingApartment = await _apartmentsRepository.GetByIdAsync(updateApartmentDTO.ID) 
                                      ?? throw new NotFoundException(nameof(Apartment), updateApartmentDTO.ID.ToString());

        var originalApartment = Apartment.Clone(existingApartment);

        _mapper.Map(updateApartmentDTO, existingApartment);
        
        await _apartmentsRepository.UpdateAsync(existingApartment, updateApartmentDTO.ApartmentPhotoUrls);

        var changeLogs = CoreUtilities.GenerateChangeLogs(originalApartment, existingApartment, currentUser.Email);
        await _changeLogsRepository.AddChangeLogs(changeLogs);

        var mappedApartment = _mapper.Map<UpdateApartmentDTO>(existingApartment);

        return mappedApartment;
    }

    public async Task DeleteApartment(int id)
    {
        var currentUser = _userContext.GetCurrentUser();
        if (currentUser == null)
        {
            _logger.LogWarning("User not found!");
            return;
        }

        _logger.LogInformation("Deleting Apartment with id : {ApartmentID}", id);
        var existingApartment = await _apartmentsRepository.GetByIdAsync(id) 
                                      ?? throw new NotFoundException(nameof(Apartment), id.ToString());

        var originalApartment = Apartment.Clone(existingApartment);
     
        await _apartmentsRepository.DeleteAsync(existingApartment);

        var apartmentsChangeLogs = CoreUtilities.GenerateChangeLogs(originalApartment, existingApartment, currentUser.Email);
        var apartmentPhotosChangeLogs = CoreUtilities.GenerateChangeLogs(originalApartment.ApartmentPhotos, existingApartment.ApartmentPhotos, currentUser.Email, ["Apartment"]);

        var changeLogs = apartmentsChangeLogs.ToList();
        changeLogs.AddRange(apartmentPhotosChangeLogs);
        await _changeLogsRepository.AddChangeLogs(changeLogs);
    }

    public async Task<List<ApartmentPhotoDTO>> GetApartmentPhotos(int id)
    {
        _logger.LogInformation($"Getting Apartment with ID = {id} Photos");
        var apartment = await _apartmentsRepository.GetByIdAsync(id)
                              ?? throw new NotFoundException(nameof(Apartment), id.ToString());
        var apartmentPhotos = apartment.ApartmentPhotos;

        var apartmentPhotoDTOs = _mapper.Map<List<ApartmentPhotoDTO>>(apartmentPhotos);

        return apartmentPhotoDTOs;
    }
}