using Apartments.Application.Common;
using Apartments.Application.Dtos.NotificationDtos;
using Apartments.Application.IServices;
using Apartments.Domain.Common;
using Apartments.Domain.Entities;
using Apartments.Domain.IRepositories;
using AutoMapper;
using FirebaseAdmin.Auth.Multitenancy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Apartments.Application.Services.ApartmentRequestHandlers;

public interface IRentRequestHandler
{
    Task<ServiceResult<string>> ApproveReject(CurrentUser currentUser, ApartmentRequest apartmentRequest,
        RequestAction requestAction);

    Task<ServiceResult<string>> SendRentRequest(CurrentUser currentUser, Apartment existingApartment);
}

public class RentRequestHandler(
    ILogger<RentRequestHandler> logger,
    IMapper mapper,
    IApartmentRepository apartmentRepository,
    IRentTransactionRepository rentTransactionRepository,
    IApartmentRequestRepository apartmentRequestRepository,
    IEmailService emailService,
    INotificationRepository notificationRepository,
    INotificationService notificationService,
    INotificationDispatcher notificationDispatcher
) : IRentRequestHandler
{
    public async Task<ServiceResult<string>> ApproveReject(CurrentUser currentUser, ApartmentRequest apartmentRequest,
        RequestAction requestAction)
    {
        await using var transaction = await apartmentRepository.BeginTransactionAsync();
        string notificationMessage = string.Empty;

        try
        {
            var targetStatus = requestAction == RequestAction.Approve ? RequestStatus.Approved : RequestStatus.Rejected;

            if (apartmentRequest.Status.Equals(targetStatus, StringComparison.OrdinalIgnoreCase))
                return ServiceResult<string>.ErrorResult(StatusCodes.Status400BadRequest,
                    $"Rent Request is already {targetStatus}");

            var alreadyATenant = await apartmentRepository.GetApartmentByTenantId(apartmentRequest.TenantId);
            if (alreadyATenant != null)
            {
                return ServiceResult<string>.ErrorResult(StatusCodes.Status400BadRequest,
                    "User is already a tenant in another apartment");
            }
            else
            {
                await apartmentRequestRepository.CancelRemainingRequests(apartmentRequest);
            }

            var originalRequest = mapper.Map<ApartmentRequest>(apartmentRequest);
            if (requestAction == RequestAction.Approve)
            {
                apartmentRequest.Status = RequestStatus.Approved;
                notificationMessage = "Rent Request approved successfully.";
                await HandleApprovedRequest(apartmentRequest, currentUser.Email, notificationMessage);
            }
            else
            {
                apartmentRequest.Status = RequestStatus.Rejected;
                notificationMessage = "Rent Request rejected successfully.";
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
                $"Failed to {requestAction.ToString()} the rent request");
        }

        try
        {
            if (!string.IsNullOrEmpty(notificationMessage))
            {
                var notificationType = NotificationType.Rent.ToString().ToLower();
                await notificationDispatcher.SendNotificationAsync(apartmentRequest.TenantId, notificationMessage,
                    notificationType, apartmentRequest.Status);

                await notificationService.SendNotificationToUser(new NotifyUserRequest()
                {
                    UserId = apartmentRequest.TenantId,
                    Title = "Rental Request",
                    Body = notificationMessage
                });

                await emailService.SendEmailAsync(apartmentRequest.Tenant.Email!, "Rent Request", notificationMessage);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send notification");
        }

        return ServiceResult<string>.InfoResult(StatusCodes.Status200OK, notificationMessage);
    }

    private async Task HandleApprovedRequest(ApartmentRequest apartmentRequest, string userEmail, string notificationMessage)
    {
        try
        {
            var notificationType = NotificationType.Rent.ToString().ToLower();
            
            // Update apartment IsOccupied = True
            var apartment = apartmentRequest.Apartment;
            var originalApartment = mapper.Map<Apartment>(apartment);
            apartment.IsOccupied = true;
            apartment.TenantId = apartmentRequest.TenantId;
            apartment.AvailableFrom = null;
            await apartmentRepository.UpdateApartmentAsync(originalApartment, apartment, userEmail);

            // Add the first RentTransaction entry
            var dateFrom = originalApartment.AvailableFrom.HasValue
                           ? originalApartment.AvailableFrom.Value
                           : DateOnly.FromDateTime(DateTime.UtcNow);

            var dateTo = dateFrom.AddMonths(1);

            var rentTransaction = new RentTransaction
            {
                TenantId = apartmentRequest.TenantId,
                OwnerId = apartmentRequest.OwnerId,
                ApartmentId = apartmentRequest.ApartmentId,
                DateFrom = dateFrom,
                DateTo = dateTo,
                RentAmount = apartment.RentAmount,
                Status = RequestStatus.Paid.ToString()
            };
            await rentTransactionRepository.AddRentTransactionAsync(rentTransaction);

            // Store it in the Db
            var notification = new Notification
            {
                UserId = apartmentRequest.TenantId,
                Message = notificationMessage,
                Type = notificationType,
                IsRead = false
            };
            await notificationRepository.AddNotificationAsync(notification);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while approving rent request");
            throw;
        }
    }
    public async Task<ServiceResult<string>> SendRentRequest(CurrentUser currentUser, Apartment existingApartment)
    {
        try
        {
            var requestType = ApartmentRequestType.Rent.ToString();

            if (existingApartment.IsOccupied)
                return ServiceResult<string>.ErrorResult(StatusCodes.Status401Unauthorized,
                    "Apartment is already rented!");

            var existingRequest =
                await apartmentRequestRepository.GetApartmentRequestWithStatusAsync(existingApartment.Id,
                    currentUser.Id, requestType);

            if (existingRequest != null)
                return ServiceResult<string>.ErrorResult(StatusCodes.Status401Unauthorized, "Request exists already!");

            var apartmentRequest = new ApartmentRequest(requestType)
            {
                TenantId = currentUser.Id,
                ApartmentId = existingApartment.Id,
                OwnerId = existingApartment.OwnerId,
                RequestDate = existingApartment.AvailableFrom
            };

            await apartmentRequestRepository.AddApartmentRequestAsync(apartmentRequest);

            // Trigger Notification
            var notificationType = NotificationType.Rent.ToString().ToLower();
            var notificationMessage = $"A new rent request has been submitted for your apartment '{existingApartment.Title}' ";
            await notificationDispatcher.SendNotificationAsync(existingApartment.OwnerId,
                notificationMessage, notificationType);

            // Store it in the Db
            var notification = new Notification
            {
                UserId = apartmentRequest.OwnerId,
                Message = notificationMessage,
                Type = notificationType,
                IsRead = false
            };
            await notificationRepository.AddNotificationAsync(notification);

            return ServiceResult<string>.InfoResult(StatusCodes.Status201Created,
                "Apartment application sent successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return ServiceResult<string>.ErrorResult(StatusCodes.Status500InternalServerError,
                "Apartment application failed.");
        }
    }
}