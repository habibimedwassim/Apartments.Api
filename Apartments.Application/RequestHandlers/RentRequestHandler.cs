using Apartments.Application.Common;
using Apartments.Application.IServices;
using Apartments.Domain.Common;
using Apartments.Domain.Entities;
using Apartments.Domain.Exceptions;
using Apartments.Domain.IRepositories;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Apartments.Application.RequestHandlers;

public interface IRentRequestHandler
{
    Task<ServiceResult<string>> ApproveReject(CurrentUser currentUser, ApartmentRequest apartmentRequest, RentTransaction latestTransaction, RequestAction requestAction);
    Task<ServiceResult<string>> SendRentRequest(CurrentUser currentUser, Apartment existingApartment);
}
public class RentRequestHandler(
    ILogger<RentRequestHandler> logger,
    IMapper mapper,
    IUserRepository userRepository,
    IApartmentRepository apartmentRepository,
    IRentTransactionRepository rentTransactionRepository,
    IApartmentRequestRepository apartmentRequestRepository
    ) : IRentRequestHandler
{
    public async Task<ServiceResult<string>> ApproveReject(CurrentUser currentUser, ApartmentRequest apartmentRequest, RentTransaction latestTransaction, RequestAction requestAction)
    {
        try
        {
            var targetStatus = requestAction == RequestAction.Approve ? RequestStatus.Approved : RequestStatus.Rejected;

            if (apartmentRequest.Status.Equals(targetStatus, StringComparison.OrdinalIgnoreCase))
            {
                return ServiceResult<string>.ErrorResult(StatusCodes.Status400BadRequest, $"Rent Request is already {targetStatus}");
            }

            var originalRequest = mapper.Map<ApartmentRequest>(apartmentRequest);
            string successMessage;

            if (requestAction == RequestAction.Approve)
            {
                apartmentRequest.Status = RequestStatus.Approved;
                successMessage = "Rent Request approved successfully.";
                await HandleApprovedRequest(apartmentRequest, latestTransaction, currentUser.Email);
            }
            else
            {
                apartmentRequest.Status = RequestStatus.Rejected;
                successMessage = "Rent Request rejected successfully.";
            }

            await apartmentRequestRepository.UpdateApartmentRequestAsync(originalRequest, apartmentRequest, currentUser.Email);

            return ServiceResult<string>.InfoResult(StatusCodes.Status200OK, successMessage);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);

            return ServiceResult<string>.ErrorResult(StatusCodes.Status500InternalServerError, $"Failed to {requestAction.ToString()} the rent request");
        }
    }

    public async Task<ServiceResult<string>> SendRentRequest(CurrentUser currentUser, Apartment existingApartment)
    {
        try
        {
            var requestType = ApartmentRequestType.Rent;

            if (existingApartment.IsOccupied)
            {
                return ServiceResult<string>.ErrorResult(StatusCodes.Status401Unauthorized, "Apartment is already rented!");
            }

            var existingRequest = await apartmentRequestRepository.GetApartmentRequestWithStatusAsync(existingApartment.Id, currentUser.Id, requestType.ToString(), RequestStatus.Pending);

            if (existingRequest != null)
            {
                return ServiceResult<string>.ErrorResult(StatusCodes.Status401Unauthorized, "Request exists already!");
            }

            var apartmentRequest = new ApartmentRequest()
            {
                TenantId = currentUser.Id,
                ApartmentId = existingApartment.Id,
                OwnerId = existingApartment.OwnerId,
                RequestType = requestType.ToString(),
            };

            await apartmentRequestRepository.AddApartmentRequestAsync(apartmentRequest);

            return ServiceResult<string>.InfoResult(StatusCodes.Status201Created, "Apartment application sent successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return ServiceResult<string>.ErrorResult(StatusCodes.Status500InternalServerError, "Apartment application failed.");
        }
    }

    private async Task HandleApprovedRequest(ApartmentRequest apartmentRequest, RentTransaction latestTransaction, string userEmail)
    {
        try
        {
            // Update the tenant's current apartment
            var user = apartmentRequest.Tenant;
            var originalUser = mapper.Map<User>(user);
            user.CurrentApartmentId = apartmentRequest.ApartmentId;
            await userRepository.UpdateAsync(originalUser, user, userEmail);

            // Update apartment IsOccupied = True
            var apartment = apartmentRequest.Apartment;
            var originalApartment = mapper.Map<Apartment>(apartment);
            apartment.IsOccupied = true;
            await apartmentRepository.UpdateApartmentAsync(originalApartment, apartment, userEmail);

            // Add the first RentTransaction entry
            var rentTransaction = new RentTransaction
            {
                TenantId = apartmentRequest.TenantId,
                ApartmentId = apartmentRequest.ApartmentId,
                DateFrom = DateOnly.FromDateTime(DateTime.UtcNow),
                DateTo = DateOnly.FromDateTime(DateTime.UtcNow).AddMonths(1),
                RentAmount = apartment.RentAmount,
                Status = RequestStatus.Paid.ToString(),
            };
            await rentTransactionRepository.AddRentTransactionAsync(rentTransaction);
        }
        catch
        {
            throw;
        }
    }
}
