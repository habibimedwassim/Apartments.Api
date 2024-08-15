using AutoMapper;
using Microsoft.Extensions.Logging;
using RenTN.Application.DTOs.ApartmentDTOs;
using RenTN.Domain.Entities;
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
        var apartment = await _apartmentsRepository.GetByIdAsync(id);
        var apartmentDTO = _mapper.Map<ApartmentDTO?>(apartment);
        return apartmentDTO;
    }

    public async Task<int> CreateApartment(CreateApartmentDTO createApartmentDTO)
    {
        _logger.LogInformation("Creating a new Apartment");

        var apartment = _mapper.Map<Apartment>(createApartmentDTO);
        apartment.Owner = new User { Email = "test-owner@test.com"};

        int id = await _apartmentsRepository.CreateAsync(apartment);
        return id;
    }
}
