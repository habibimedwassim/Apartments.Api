using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using RenTN.Application.DTOs.RentalRequestDTOs;
using RenTN.Application.Users;
using RenTN.Application.Utilities;
using RenTN.Domain.Common;
using RenTN.Domain.Entities;
using RenTN.Domain.Exceptions;
using RenTN.Domain.Interfaces;

namespace RenTN.Application.Services.RentalRequestService;

public class RentalRequestService(
    ILogger<RentalRequestService> _logger,
    IMapper _mapper,
    IUserContext _userContext,
    UserManager<User> _userManager,
    IRentalRequestsRepository _rentalRequestsRepository,
    IApartmentsRepository _apartmentsRepository
    ) : IRentalRequestService
{
    public async Task<IEnumerable<RentalRequestDTO>> GetAllRentalRequests(RentalRequestType requestType)
    {
        var currentUser = _userContext.GetCurrentUser();
        if (currentUser == null)
        {
            _logger.LogWarning("User not found!");
            return Enumerable.Empty<RentalRequestDTO>();
        }

        var user = await _userManager.FindByEmailAsync(currentUser.Email) ??
                   throw new NotFoundException(nameof(User), currentUser.Id);

        var requests = await _rentalRequestsRepository.GetAllAsync(user.Id, requestType);
        var requestsDTOs = _mapper.Map<IEnumerable<RentalRequestDTO>>(requests);
        return requestsDTOs;
    }
    public async Task<ApplicationResponse> SendRentalRequest(CreateRentalRequestDTO createRentalRequestDTO)
    {
        var currentUser = _userContext.GetCurrentUser();
        if (currentUser == null)
        {
            _logger.LogWarning("User not found!");
            return new ApplicationResponse(false, 401, "User not authenticated");
        }

        var tenant = await _userManager.FindByEmailAsync(currentUser.Email);
        if (tenant == null)
        {
            _logger.LogWarning("Tenant with email {Email} not found!", currentUser.Email);
            return new ApplicationResponse(false, 404, "Tenant not found");
        }

        var existingRequest = await _rentalRequestsRepository.GetByTenantAndApartmentIdAsync(tenant.Id, createRentalRequestDTO.ApartmentID);
        if (existingRequest != null)
        {
            return new ApplicationResponse(false, 409, "You have already sent a rental request for this apartment.");
        }

        var requestedApartment = await _apartmentsRepository.GetByIdAsync(createRentalRequestDTO.ApartmentID);
        if (requestedApartment == null)
        {
            return new ApplicationResponse(false, 404, $"Apartment with id {createRentalRequestDTO.ApartmentID} not found");
        }

        var rentalRequest = new RentalRequest
        {
            TenantID = tenant.Id,
            ApartmentID = requestedApartment.ID,
            OwnerID = requestedApartment.OwnerID,
            RequestDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Status = RentalRequestStatus.Pending,
            IsDeleted = false
        };

        await _rentalRequestsRepository.CreateAsync(rentalRequest);

        _logger.LogInformation("Rental request created for Tenant: {TenantId}, Apartment: {ApartmentId}", tenant.Id, createRentalRequestDTO.ApartmentID);

        return new ApplicationResponse(true, 201, "Rental request sent successfully.");
    }
}
