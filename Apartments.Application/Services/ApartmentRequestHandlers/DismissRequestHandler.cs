using Apartments.Application.Common;
using Apartments.Application.Dtos.ApartmentRequestDtos;
using Apartments.Application.IServices;
using Apartments.Domain.Common;
using Apartments.Domain.Entities;
using Apartments.Domain.IRepositories;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Apartments.Application.Services.ApartmentRequestHandlers;

public interface IDismissRequestHandler
{
    Task<ServiceResult<string>> DismissTenant(CurrentUser currentUser, Apartment apartment, User tenant,
        RentTransaction latestRentTransaction, LeaveDismissRequestDto dismissRequest);
}

public class DismissRequestHandler(
    ILogger<DismissRequestHandler> logger,
    IMapper mapper,
    IEmailService emailService,
    IApartmentRepository apartmentRepository,
    IRentTransactionRepository rentTransactionRepository,
    IApartmentRequestRepository apartmentRequestRepository
) : IDismissRequestHandler
{
    public async Task<ServiceResult<string>> DismissTenant(CurrentUser currentUser, Apartment apartment, User tenant,
        RentTransaction latestRentTransaction, LeaveDismissRequestDto dismissRequest)
    {
        await using var transaction = await apartmentRepository.BeginTransactionAsync();

        try
        {
            logger.LogInformation("Dismissing Tenant with Id = {TenantId} from Apartment with Id = {ApartmentId}",
                tenant.SysId, apartment.Id);

            var requestDate = dismissRequest.RequestDate!.Value;

            // Update the apartment's record
            var originalApartment = mapper.Map<Apartment>(apartment);
            apartment.IsOccupied = false;
            apartment.TenantId = null;
            apartment.AvailableFrom = requestDate;
            await apartmentRepository.UpdateApartmentAsync(originalApartment, apartment, currentUser.Email);

            // Create a dismiss tenant request record
            var dismissTenantRequest = new ApartmentRequest(ApartmentRequestType.Dismiss.ToString())
            {
                TenantId = tenant.Id,
                ApartmentId = apartment.Id,
                OwnerId = apartment.OwnerId,
                RequestDate = requestDate,
                Reason = dismissRequest.Reason,
                Status = RequestStatus.Terminated
            };
            await apartmentRequestRepository.AddApartmentRequestAsync(dismissTenantRequest);

            // Update the tenant's rent transaction
            var rentTransaction = new RentTransaction
            {
                TenantId = tenant.Id,
                OwnerId = apartment.OwnerId,
                ApartmentId = apartment.Id,
                DateFrom = requestDate,
                DateTo = null,
                RentAmount = apartment.RentAmount,
                Status = RequestStatus.Terminated
            };
            await rentTransactionRepository.DeletePendingRentTransactionsAsync(latestRentTransaction);

            await rentTransactionRepository.AddRentTransactionAsync(rentTransaction);

            await transaction.CommitAsync();
            // Notify tenant about the dismissal
            var message =
                $"You have been dismissed from the apartment {apartment.Description}, you have until ({requestDate.ToString(AppConstants.DateFormat)}) to clear the apartment. Reason: {dismissRequest.Reason}";
            await emailService.SendEmailAsync(tenant.Email!, "Dismissed from Apartment", message);

            return ServiceResult<string>.SuccessResult("Tenant dismissed successfully");
        }
        catch
        {
            await transaction.RollbackAsync();
            return ServiceResult<string>.ErrorResult(StatusCodes.Status500InternalServerError,
                "Failed to dismiss the tenant");
        }
    }
}