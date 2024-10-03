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

namespace Apartments.Application.Services.ApartmentRequestHandlers;

public interface ILeaveRequestHandler
{
    Task<ServiceResult<string>> ApproveReject(CurrentUser currentUser, ApartmentRequest apartmentRequest,
        RequestAction requestAction);

    Task<ServiceResult<string>> SendLeaveRequest(CurrentUser currentUser, Apartment apartment, LeaveDismissRequestDto leaveRequestDto);
}

public class LeaveRequestHandler(
    ILogger<LeaveRequestHandler> logger,
    IEmailService emailService,
    IMapper mapper,
    IApartmentRequestRepository apartmentRequestRepository,
    IApartmentRepository apartmentRepository,
    IRentTransactionRepository rentTransactionRepository
) : ILeaveRequestHandler
{
    public async Task<ServiceResult<string>> ApproveReject(CurrentUser currentUser, ApartmentRequest apartmentRequest,
        RequestAction requestAction)
    {
        await using var transaction = await apartmentRepository.BeginTransactionAsync();

        try
        {
            var latestTransaction =
            await rentTransactionRepository.GetLatestRentTransactionAsync(apartmentRequest.ApartmentId,
                apartmentRequest.TenantId) ??
            throw new NotFoundException("No Rent Transactions were found for this user");

            var targetStatus = requestAction == RequestAction.Approve ? RequestStatus.Approved : RequestStatus.Rejected;

            if (apartmentRequest.Status.Equals(targetStatus, StringComparison.OrdinalIgnoreCase))
                return ServiceResult<string>.ErrorResult(StatusCodes.Status400BadRequest,
                    $"Leave Request is already {targetStatus}");

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

            await apartmentRequestRepository.UpdateApartmentRequestAsync(originalRequest, apartmentRequest,
                currentUser.Email);

            await transaction.CommitAsync();
            return ServiceResult<string>.InfoResult(StatusCodes.Status200OK, successMessage);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            logger.LogError(ex, ex.Message);

            return ServiceResult<string>.ErrorResult(StatusCodes.Status500InternalServerError,
                $"Failed to {requestAction.ToString()} the leave request");
        }
    }

    private async Task HandleApprovedRequest(ApartmentRequest apartmentRequest, RentTransaction latestTransaction,
        string userEmail)
    {
        try
        {
            var dateFrom = apartmentRequest.RequestDate ?? DateOnly.FromDateTime(DateTime.UtcNow);

            // Update apartment IsOccupied = True
            var apartment = apartmentRequest.Apartment;
            var originalApartment = mapper.Map<Apartment>(apartment);
            apartment.IsOccupied = false;
            apartment.TenantId = null;
            apartment.AvailableFrom = dateFrom;
            await apartmentRepository.UpdateApartmentAsync(originalApartment, apartment, userEmail);

            // Update latest transaction
            var rentTransaction = new RentTransaction
            {
                TenantId = apartmentRequest.TenantId,
                OwnerId = apartment.OwnerId,
                ApartmentId = apartment.Id,
                DateFrom = dateFrom,
                DateTo = null,
                RentAmount = apartment.RentAmount,
                Status = RequestStatus.Departed,
            };
            await rentTransactionRepository.DeletePendingRentTransactionsAsync(latestTransaction);

            await rentTransactionRepository.AddRentTransactionAsync(rentTransaction);
        }
        catch
        {
            throw;
        }
    }

    public async Task<ServiceResult<string>> SendLeaveRequest(CurrentUser currentUser, Apartment apartment,
        LeaveDismissRequestDto leaveRequestDto)
    {
        try
        {
            logger.LogInformation("Attempting to leave Apartment with Id = {ApartmentId}", apartment.Id);

            // Create a leave tenant request record
            var leaveRequest = new ApartmentRequest(ApartmentRequestType.Leave.ToString())
            {
                TenantId = currentUser.Id,
                ApartmentId = apartment.Id,
                OwnerId = apartment.OwnerId,
                RequestDate = leaveRequestDto.RequestDate!.Value,
                Reason = leaveRequestDto.Reason,
            };
            await apartmentRequestRepository.AddApartmentRequestAsync(leaveRequest);

            // Notify owner about the leave request
            var message =
                $"Tenant ({currentUser.Email}) has requested to leave the apartment {apartment.Description}. Reason: {leaveRequestDto.Reason}";
            await emailService.SendEmailAsync(currentUser.Email, "Request to leave Apartment", message);

            return ServiceResult<string>.SuccessResult("Leave request sent successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return ServiceResult<string>.ErrorResult(StatusCodes.Status500InternalServerError,
                "Failed to send the leave request");
        }
    }
}