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


public interface ILeaveRequestHandler
{
    Task<ServiceResult<string>> ApproveReject(CurrentUser currentUser, ApartmentRequest apartmentRequest, RentTransaction latestTransaction, RequestAction requestAction);
    Task<ServiceResult<string>> SendLeaveRequest(CurrentUser currentUser, Apartment apartment, string leaveReason);
}
public class LeaveRequestHandler(
    ILogger<LeaveRequestHandler> logger,
    IEmailService emailService,
    IMapper mapper,
    IUserRepository userRepository,
    IApartmentRequestRepository apartmentRequestRepository,
    IApartmentRepository apartmentRepository,
    IRentTransactionRepository rentTransactionRepository

    ) : ILeaveRequestHandler
{

    public async Task<ServiceResult<string>> ApproveReject(CurrentUser currentUser, ApartmentRequest apartmentRequest, RentTransaction latestTransaction, RequestAction requestAction)
    {
        try
        {
            var targetStatus = requestAction == RequestAction.Approve ? RequestStatus.Approved : RequestStatus.Rejected;

            if (apartmentRequest.Status.Equals(targetStatus, StringComparison.OrdinalIgnoreCase))
            {
                return ServiceResult<string>.ErrorResult(StatusCodes.Status400BadRequest, $"Leave Request is already {targetStatus}");
            }

            var originalRequest = mapper.Map<ApartmentRequest>(apartmentRequest);
            string successMessage;

            if (requestAction == RequestAction.Approve)
            {
                apartmentRequest.Status = RequestStatus.Approved;
                successMessage = "Leave Request approved successfully.";
                await HandleApprovedRequest(apartmentRequest, latestTransaction, currentUser.Email);
            }
            else
            {
                apartmentRequest.Status = RequestStatus.Rejected;
                successMessage = "Leave Request rejected successfully.";
            }

            await apartmentRequestRepository.UpdateApartmentRequestAsync(originalRequest, apartmentRequest, currentUser.Email);

            return ServiceResult<string>.InfoResult(StatusCodes.Status200OK, successMessage);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);

            return ServiceResult<string>.ErrorResult(StatusCodes.Status500InternalServerError, $"Failed to {requestAction.ToString()} the leave request");
        }
    }
    private async Task HandleApprovedRequest(ApartmentRequest apartmentRequest, RentTransaction latestTransaction, string userEmail)
    {
        try
        {
            // Update the tenant's current apartment
            var user = apartmentRequest.Tenant;
            var originalUser = mapper.Map<User>(user);
            user.CurrentApartmentId = null;
            await userRepository.UpdateAsync(originalUser, user, userEmail);

            // Update apartment IsOccupied = True
            var apartment = apartmentRequest.Apartment;
            var originalApartment = mapper.Map<Apartment>(apartment);
            apartment.IsOccupied = false;
            await apartmentRepository.UpdateApartmentAsync(originalApartment, apartment, userEmail);

            // Update latest transaction
            var originalTransaction = mapper.Map<RentTransaction>(latestTransaction);
            latestTransaction.DateTo = DateOnly.FromDateTime(DateTime.UtcNow);
            latestTransaction.Status = RequestStatus.Departed;
            await rentTransactionRepository.UpdateRentTransactionAsync(originalTransaction, latestTransaction, userEmail);
        }
        catch
        {
            throw;
        }
    }
    public async Task<ServiceResult<string>> SendLeaveRequest(CurrentUser currentUser, Apartment apartment, string leaveReason)
    {
        try
        {
            logger.LogInformation("Attempting to leave Apartment with Id = {ApartmentId}", apartment.Id);

            // Create a leave tenant request record
            var dismissRequest = new ApartmentRequest()
            {
                TenantId = currentUser.Id,
                ApartmentId = apartment.Id,
                OwnerId = apartment.OwnerId,
                Reason = leaveReason,
                RequestType = ApartmentRequestType.Leave.ToString(),
            };
            await apartmentRequestRepository.AddApartmentRequestAsync(dismissRequest);

            // Notify owner about the leave request
            var message = $"Tenant ({currentUser.Email}) has requested to leave the apartment {apartment.Description}. Reason: {leaveReason}";
            await emailService.SendEmailAsync(currentUser.Email, "Request to leave Apartment", message);

            return ServiceResult<string>.SuccessResult("Leave request sent successfully");
        }
        catch(Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return ServiceResult<string>.ErrorResult(StatusCodes.Status500InternalServerError, "Failed to send the leave request");
        }
    }
}
