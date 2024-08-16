using AutoMapper;
using Microsoft.Extensions.Logging;
using RenTN.Application.DTOs.ApartmentDTOs;
using RenTN.Domain.Entities;
using RenTN.Domain.Exceptions;
using RenTN.Domain.Interfaces;

namespace RenTN.Application.Services.ApartmentsService;

internal class ApartmentsService(
    ILogger<ApartmentsService> _logger,
    IMapper _mapper,
    IApartmentsRepository _apartmentsRepository) : IApartmentsService
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

    public async Task<int> CreateApartment(CreateApartmentDTO createApartmentDTO)
    {
        _logger.LogInformation("Creating a new Apartment");

        var apartment = _mapper.Map<Apartment>(createApartmentDTO);
        apartment.OwnerID = "0e0f9d35-e5fa-4e56-86cf-c1e2f1cfe2a5";

        int id = await _apartmentsRepository.CreateAsync(apartment);
        return id;
    }

    public async Task UpdateApartment(UpdateApartmentDTO updateApartmentDTO)
    {
        _logger.LogInformation("Updating Apartment with Id = {ApartmentId}", updateApartmentDTO.ID);

        var existingApartment = await _apartmentsRepository.GetByIdAsync(updateApartmentDTO.ID) 
                                      ?? throw new NotFoundException(nameof(Apartment), updateApartmentDTO.ID.ToString());

        _mapper.Map(updateApartmentDTO, existingApartment);

        await _apartmentsRepository.UpdateAsync(existingApartment, updateApartmentDTO.ApartmentPhotoUrls);
    }

    public async Task DeleteApartment(int id)
    {
        _logger.LogInformation("Deleting Apartment with id : {ApartmentID}", id);
        var existingApartment = await _apartmentsRepository.GetByIdAsync(id) 
                                      ?? throw new NotFoundException(nameof(Apartment), id.ToString());

        await _apartmentsRepository.DeleteAsync(existingApartment);
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