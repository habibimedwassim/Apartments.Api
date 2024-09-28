using Apartments.Application.Common;
using Apartments.Application.Dtos.ApartmentRequestDtos;
using Apartments.Application.IServices;
using Apartments.Domain.Common;
using Apartments.Domain.Entities;
using Apartments.Domain.Exceptions;
using Apartments.Domain.IRepositories;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Apartments.Application.RequestHandlers;
public interface IDismissRequestHandler
{
    Task<ServiceResult<string>> DismissTenant(CurrentUser currentUser, Apartment apartment, User tenant, RentTransaction latestRentTransaction, string dismissReason);
}
public class DismissRequestHandler(
    ILogger<DismissRequestHandler> logger,
    IMapper mapper,
    IEmailService emailService,
    IUserRepository userRepository,
    IApartmentRepository apartmentRepository,
    IRentTransactionRepository rentTransactionRepository,
    IApartmentRequestRepository apartmentRequestRepository
    ) : IDismissRequestHandler
{
    public async Task<ServiceResult<string>> DismissTenant(CurrentUser currentUser, Apartment apartment, User tenant, RentTransaction latestRentTransaction, string dismissReason)
    {
        try
        {
            logger.LogInformation("Dismissing Tenant with Id = {TenantId} from Apartment with Id = {ApartmentId}", tenant.SysId, apartment.Id);

            // Update the tenant's rent transaction
            var originalRentTransaction = mapper.Map<RentTransaction>(latestRentTransaction);
            latestRentTransaction.DateTo = DateOnly.FromDateTime(DateTime.UtcNow);
            latestRentTransaction.Status = RequestStatus.Terminated;
            await rentTransactionRepository.UpdateRentTransactionAsync(originalRentTransaction, latestRentTransaction, currentUser.Email);

            // Update the tenant's record
            var originalRecord = mapper.Map<User>(tenant);
            tenant.CurrentApartmentId = null;
            await userRepository.UpdateAsync(originalRecord, tenant, currentUser.Email);

            // Update the apartment's record
            var originalApartment = mapper.Map<Apartment>(apartment);
            apartment.IsOccupied = false;
            await apartmentRepository.UpdateApartmentAsync(originalApartment, apartment, currentUser.Email);

            // Create a dismiss tenant request record
            var dismissRequest = new ApartmentRequest()
            {
                TenantId = tenant.Id,
                ApartmentId = apartment.Id,
                OwnerId = apartment.OwnerId,
                Reason = dismissReason,
                Status = RequestStatus.Terminated,
                RequestType = ApartmentRequestType.Dismiss.ToString(),
            };
            await apartmentRequestRepository.AddApartmentRequestAsync(dismissRequest);

            // Notify tenant about the dismissal
            var message = $"You have been removed from the apartment {apartment.Description}, you have until the end of the month to clear the apartment. Reason: {dismissReason}";
            await emailService.SendEmailAsync(tenant.Email!, "Dismissed from Apartment", message);

            return ServiceResult<string>.SuccessResult("Tenant dismissed successfully");
        }
        catch
        {
            return ServiceResult<string>.ErrorResult(StatusCodes.Status500InternalServerError, "Failed to dismiss the tenant");
        }
    }
}
