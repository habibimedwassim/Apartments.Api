using Apartments.Application.Common;
using Apartments.Application.Dtos.ApartmentRequestDtos;
using Apartments.Application.Dtos.NotificationDtos;
using Apartments.Application.IServices;
using Apartments.Application.Utilities;
using Apartments.Domain.Common;
using Apartments.Domain.Entities;
using Apartments.Domain.Exceptions;
using Apartments.Domain.IRepositories;
using AutoMapper;
using FirebaseAdmin.Auth.Multitenancy;
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
    IRentTransactionRepository rentTransactionRepository,
    INotificationUtilities notificationUtilities
) : ILeaveRequestHandler
{
    public async Task<ServiceResult<string>> ApproveReject(CurrentUser currentUser, ApartmentRequest apartmentRequest,
        RequestAction requestAction)
    {
        await using var transaction = await apartmentRepository.BeginTransactionAsync();
        string notificationMessage = string.Empty;
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
            if (requestAction == RequestAction.Approve)
            {
                apartmentRequest.Status = RequestStatus.Approved;
                notificationMessage = "Leave Request approved.";
                await HandleApprovedRequest(apartmentRequest, latestTransaction, currentUser.Email, notificationMessage);
            }
            else
            {
                apartmentRequest.Status = RequestStatus.Rejected;
                notificationMessage = "Leave Request rejected.";
            }

            await apartmentRequestRepository.UpdateApartmentRequestAsync(originalRequest, apartmentRequest,
                currentUser.Email);

            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            logger.LogError(ex, ex.Message);

            return ServiceResult<string>.ErrorResult(StatusCodes.Status500InternalServerError,
                $"Failed to {requestAction.ToString()} the leave request");
        }

        try
        {
            if (!string.IsNullOrEmpty(notificationMessage))
            {
                var notificationModel = new NotificationModel()
                {
                    UserId = apartmentRequest.TenantId,
                    Email = apartmentRequest.Tenant.Email!,
                    Title = "Leave Request",
                    Message = notificationMessage,
                    NotificationType = NotificationType.Leave.ToString().ToLower(),
                    Status = apartmentRequest.Status
                };

                await notificationUtilities.SendNotificationAsync(notificationModel);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send notification");
        }

        return ServiceResult<string>.InfoResult(StatusCodes.Status200OK, notificationMessage);
    }

    private async Task HandleApprovedRequest(ApartmentRequest apartmentRequest, RentTransaction latestTransaction,
        string userEmail, string notificationMessage)
    {
        try
        {
            var notificationType = NotificationType.Leave.ToString().ToLower();
            var dateFrom = apartmentRequest.RequestDate ?? DateOnly.FromDateTime(DateTime.UtcNow);

            // Update apartment IsOccupied = True
            var apartment = apartmentRequest.Apartment;
            var tenant = apartmentRequest.Tenant;
            var originalApartment = mapper.Map<Apartment>(apartment);
            apartment.IsOccupied = false;
            apartment.TenantId = null;
            apartment.AvailableFrom = dateFrom.AddDays(1);
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

            //// Store it in the Db
            //var notification = new Notification
            //{
            //    UserId = tenant.Id,
            //    Message = notificationMessage,
            //    Type = notificationType,
            //    IsRead = false
            //};
            //await notificationRepository.AddNotificationAsync(notification);
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
            var requestType = ApartmentRequestType.Leave.ToString();

            logger.LogInformation("Attempting to leave Apartment with Id = {ApartmentId}", apartment.Id);

            var existingRequest =
                await apartmentRequestRepository.GetApartmentRequestWithStatusAsync(apartment.Id,
                    currentUser.Id, requestType);

            if (existingRequest != null)
                return ServiceResult<string>.ErrorResult(StatusCodes.Status401Unauthorized, "Request exists already!");

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
                $"Tenant ({currentUser.Email}) has requested to leave the apartment titled : {apartment.Title}. Reason: {leaveRequestDto.Reason}";
            await emailService.SendEmailAsync(apartment.Owner.Email!, "Request to leave Apartment", message);

            // Trigger Notification
            var notificationMessage = $"The tenant of the apartment '{apartment.Title}' has requested to leave";

            var notificationModel = new NotificationModel()
            {
                UserId = apartment.OwnerId,
                Email = apartment.Owner.Email!,
                Title = "Leave Request",
                Message = notificationMessage,
                NotificationType = NotificationType.Leave.ToString().ToLower(),
                SendFirebase = false,
                SendEmail = false
            };

            await notificationUtilities.SendNotificationAsync(notificationModel);

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