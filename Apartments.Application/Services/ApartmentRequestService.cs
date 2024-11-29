using Apartments.Application.Common;
using Apartments.Application.Dtos.ApartmentRequestDtos;
using Apartments.Application.Dtos.NotificationDtos;
using Apartments.Application.Dtos.UserDtos;
using Apartments.Application.IServices;
using Apartments.Application.Services.ApartmentRequestHandlers;
using Apartments.Application.Utilities;
using Apartments.Domain.Common;
using Apartments.Domain.Entities;
using Apartments.Domain.Exceptions;
using Apartments.Domain.IRepositories;
using Apartments.Domain.QueryFilters;
using AutoMapper;
using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Apartments.Application.Services;

public class ApartmentRequestService(
    ILogger<ApartmentRequestService> logger,
    IMapper mapper,
    IUserContext userContext,
    IAuthorizationManager authorizationManager,
    IEmailService emailService,
    INotificationService notificationService,
    IDismissRequestHandler dismissTenantHandler,
    ILeaveRequestHandler leaveRequestHandler,
    IRentRequestHandler rentRequestHandler,
    IUserRepository userRepository,
    IApartmentRepository apartmentRepository,
    IApartmentRequestRepository apartmentRequestRepository,
    IRentTransactionRepository rentTransactionRepository,
    INotificationRepository notificationRepository,
    INotificationDispatcher notificationDispatcher)
    : IApartmentRequestService
{
    public async Task<ServiceResult<string>> ApplyForApartment(int apartmentId)
    {
        var currentUser = userContext.GetCurrentUser();

        if (!authorizationManager.AuthorizeApartmentRequest(currentUser, ResourceOperation.Create,
                ApartmentRequestType.Rent)) throw new ForbiddenException("Not authorized to apply for an apartment");

        logger.LogInformation("Applying for Apartment with ID = {ApartmentId}", apartmentId);

        var userApartment = await apartmentRepository.GetApartmentByTenantId(currentUser.Id);

        if(userApartment != null)
        {
            return ServiceResult<string>.ErrorResult(StatusCodes.Status401Unauthorized, 
                "Leave your current apartment before applying for a new one");
        }

        var existingApartment = await apartmentRepository.GetApartmentByIdAsync(apartmentId) ??
                                throw new NotFoundException(nameof(Apartment), apartmentId.ToString());

        return await rentRequestHandler.SendRentRequest(currentUser, existingApartment);
    }

    public async Task<PagedResult<ApartmentRequestDto>> GetApartmentRequestsPaged(
        ApartmentRequestPagedQueryFilter apartmentRequestQueryFilter)
    {
        var apartmentRequestType = CoreUtilities.ValidateEnum<ApartmentRequestType>(apartmentRequestQueryFilter.type);

        var currentUser = userContext.GetCurrentUser();

        logger.LogInformation($"Retrieving {apartmentRequestType.ToString()} Requests");

        var requestType = currentUser.IsAdmin ? RequestType.All :
            currentUser.IsOwner ? RequestType.Received :
            RequestType.Sent;

        var pagedModel =
            await apartmentRequestRepository.GetApartmentRequestsPagedAsync(apartmentRequestQueryFilter, requestType,
                currentUser.Id);
        var apartmentsDto = mapper.Map<IEnumerable<ApartmentRequestDto>>(pagedModel.Data);

        var result = new PagedResult<ApartmentRequestDto>(apartmentsDto, pagedModel.DataCount,
            apartmentRequestQueryFilter.pageNumber);

        return result;
    }
    public async Task<ServiceResult<string>> ScheduleMeeting(int id, MeetingDateDto meetingDate)
    {
        var currentUser = userContext.GetCurrentUser();

        var apartmentRequest = await apartmentRequestRepository.GetApartmentRequestByIdAsync(id) ??
                               throw new NotFoundException(nameof(ApartmentRequest), id.ToString());

        var tenant = apartmentRequest.Tenant ?? throw new NotFoundException("User not found!");
        var owner = apartmentRequest.Owner ?? throw new NotFoundException("User not found!");

        if (apartmentRequest.RequestType != ApartmentRequestType.Rent.ToString())
        {
            return ServiceResult<string>.ErrorResult(StatusCodes.Status400BadRequest, 
                "Only rent requests can have a scheduled meeting!");
        }

        var originalRecord = mapper.Map<ApartmentRequest>(apartmentRequest);
        apartmentRequest.Status = RequestStatus.MeetingScheduled;
        apartmentRequest.RequestDate = meetingDate.MeetingDate;

        await apartmentRequestRepository.UpdateApartmentRequestAsync(originalRecord, apartmentRequest, currentUser.Email);

        // Trigger Notification
        var notificationType = NotificationType.Rent.ToString().ToLower();
        var notificationMessage = $"A meeting for Apartment '{apartmentRequest.Apartment.Title}' has been scheduled " +
            $"on {meetingDate.MeetingDate.ToString()}";
        await notificationDispatcher.SendNotificationAsync(tenant.Id,
            notificationMessage, notificationType);

        // Store it in the Db
        var notification = new Notification
        {
            UserId = tenant.Id,
            Message = notificationMessage,
            Type = notificationType,
            IsRead = false
        };
        await notificationRepository.AddNotificationAsync(notification);

        var emailMessage = notificationMessage + 
                            $"<br/>For more details, you can contact {owner.Email} or give them a call at +216-{owner.PhoneNumber}";
        await emailService.SendEmailAsync(tenant.Email!, "Meeting Scheduled", emailMessage);

        await notificationService.SendNotificationToUser(new NotifyUserRequest()
        {
            UserId = tenant.Id,
            Title = "Meeting Scheduled",
            Body = notificationMessage
        });
        return ServiceResult<string>.InfoResult(StatusCodes.Status200OK, "Meeting Scheduled");
    }
    public async Task<ServiceResult<IEnumerable<ApartmentRequestDto>>> GetApartmentRequests(
        ApartmentRequestQueryFilter apartmentRequestQueryFilter)
    {
        var apartmentRequestType = CoreUtilities.ValidateEnum<ApartmentRequestType>(apartmentRequestQueryFilter.type);

        var currentUser = userContext.GetCurrentUser();

        logger.LogInformation($"Retrieving {apartmentRequestType.ToString()} Requests");

        var requestType = currentUser.IsAdmin ? RequestType.All :
            currentUser.IsOwner ? RequestType.Received :
            RequestType.Sent;

        var apartments =
            await apartmentRequestRepository.GetApartmentRequestsAsync(apartmentRequestQueryFilter, requestType,
                currentUser.Id);

        var apartmentsDto = mapper.Map<IEnumerable<ApartmentRequestDto>>(apartments);

        return ServiceResult<IEnumerable<ApartmentRequestDto>>.SuccessResult(apartmentsDto);
    }
    public async Task<ServiceResult<UserDto>> GetTenantByRequestId(int id)
    {
        logger.LogInformation("Getting Tenant By Request with Id = {Id} ", id);
        var apartmentRequest = await apartmentRequestRepository.GetApartmentRequestByIdAsync(id) ??
                               throw new NotFoundException(nameof(ApartmentRequest), id.ToString());

        var tenant = apartmentRequest.Tenant ?? 
                               throw new NotFoundException("User not found!");

        var userDto = mapper.Map<UserDto>(tenant);
        return ServiceResult<UserDto>.SuccessResult(userDto);
    }
    public async Task<ServiceResult<ApartmentRequestDto>> GetApartmentRequestById(int requestId)
    {
        var currentUser = userContext.GetCurrentUser();

        logger.LogInformation("Getting Apartment Request with Id = {Id} ", requestId);

        var apartmentRequest = await apartmentRequestRepository.GetApartmentRequestByIdAsync(requestId) ??
                               throw new NotFoundException(nameof(ApartmentRequest), requestId.ToString());

        var apartmentRequestDto = mapper.Map<ApartmentRequestDto>(apartmentRequest);

        return ServiceResult<ApartmentRequestDto>.SuccessResult(apartmentRequestDto);
    }

    public async Task<ServiceResult<ApartmentRequestDto>> UpdateApartmentRequest(int requestId,
        UpdateApartmentRequestDto updateApartmentRequestDto)
    {
        var currentUser = userContext.GetCurrentUser();

        logger.LogInformation("Updating Apartment Request with Id = {Id} ", requestId);

        var apartmentRequest = await apartmentRequestRepository.GetApartmentRequestByIdAsync(requestId) ??
                               throw new NotFoundException(nameof(ApartmentRequest), requestId.ToString());

        var apartmentRequestType = CoreUtilities.ValidateEnum<ApartmentRequestType>(apartmentRequest.RequestType);

        if (!authorizationManager.AuthorizeApartmentRequest(currentUser, ResourceOperation.Update, apartmentRequestType,
                apartmentRequest)) throw new ForbiddenException();

        var originalRequest = mapper.Map<ApartmentRequest>(apartmentRequest);
        mapper.Map(updateApartmentRequestDto, apartmentRequest);

        if (updateApartmentRequestDto.Status != null)
        {
            apartmentRequest.Status = updateApartmentRequestDto.Status;
        }

        await apartmentRequestRepository.UpdateApartmentRequestAsync(originalRequest, apartmentRequest,
            currentUser.Email);

        return ServiceResult<ApartmentRequestDto>.InfoResult(StatusCodes.Status200OK,
            "Apartment Request Updated successfully.");
    }

    public async Task<ServiceResult<string>> DismissTenantById(int userId, LeaveDismissRequestDto dismissRequestDto)
    {
        dismissRequestDto.RequestDate = GetValidRequestDate(dismissRequestDto.RequestDate);

        var currentUser = userContext.GetCurrentUser();

        logger.LogInformation("Dismissing tenant with Id = {Id}", userId);

        var tenant = await userRepository.GetBySysIdAsync(userId) ??
                     throw new NotFoundException("User not found!");

        var apartment = await apartmentRepository.GetApartmentByTenantId(tenant.Id) ??
                        throw new BadRequestException($"Tenant ({tenant.Email}) doesn't have a current apartment yet");

        var latestRentTransaction =
            await rentTransactionRepository.GetLatestRentTransactionAsync(apartment.Id, tenant.Id) ??
            throw new NotFoundException("No Rent Transactions were found for this user");

        var apartmentRequest = new ApartmentRequest(ApartmentRequestType.Dismiss.ToString())
        {
            Apartment = apartment,
            OwnerId = apartment.OwnerId
        };

        if (!authorizationManager.AuthorizeApartmentRequest(currentUser, ResourceOperation.Create,
                ApartmentRequestType.Dismiss, apartmentRequest) ||
            !authorizationManager.AuthorizeRentTransaction(currentUser, ResourceOperation.Update,
                latestRentTransaction))
            throw new ForbiddenException();

        return await dismissTenantHandler.DismissTenant(currentUser, apartment, tenant, latestRentTransaction,
            dismissRequestDto);
    }

    public async Task<ServiceResult<string>> DismissTenantFromApartment(int apartmentId,
        LeaveDismissRequestDto dismissRequestDto)
    {
        dismissRequestDto.RequestDate = GetValidRequestDate(dismissRequestDto.RequestDate);

        var currentUser = userContext.GetCurrentUser();

        logger.LogInformation("Dismissing tenant from Apartment with Id = {Id}", apartmentId);

        var apartment = await apartmentRepository.GetApartmentByIdAsync(apartmentId) ??
                        throw new NotFoundException(nameof(Apartment), apartmentId.ToString());

        var tenant = await userRepository.GetTenantByApartmentId(apartmentId) ??
                     throw new NotFoundException($"Apartment with id = {apartmentId} doesn't have a tenant");

        var latestRentTransaction =
            await rentTransactionRepository.GetLatestRentTransactionAsync(apartmentId, tenant.Id) ??
            throw new NotFoundException("No Rent Transactions were found for this user");

        var apartmentRequest = new ApartmentRequest(ApartmentRequestType.Dismiss.ToString())
        {
            Apartment = apartment,
            OwnerId = apartment.OwnerId
        };

        if (!authorizationManager.AuthorizeApartmentRequest(currentUser, ResourceOperation.Create,
                ApartmentRequestType.Dismiss, apartmentRequest) ||
            !authorizationManager.AuthorizeRentTransaction(currentUser, ResourceOperation.Update,
                latestRentTransaction))
            throw new ForbiddenException();


        return await dismissTenantHandler.DismissTenant(currentUser, apartment, tenant, latestRentTransaction,
            dismissRequestDto);
    }

    public async Task<ServiceResult<string>> CancelApartmentRequest(int requestId)
    {
        var currentUser = userContext.GetCurrentUser();

        logger.LogInformation("Cancelling request with Id = {Id}", requestId);

        var apartmentRequest = await apartmentRequestRepository.GetApartmentRequestByIdAsync(requestId) ??
                               throw new NotFoundException("Apartment Request", requestId.ToString());

        var apartmentRequestType = CoreUtilities.ValidateEnum<ApartmentRequestType>(apartmentRequest.RequestType);

        if (!authorizationManager.AuthorizeApartmentRequest(currentUser, ResourceOperation.Cancel, apartmentRequestType,
                apartmentRequest)) throw new ForbiddenException();

        var originalRecord = mapper.Map<ApartmentRequest>(apartmentRequest);
        apartmentRequest.IsDeleted = true;
        apartmentRequest.Status = RequestStatus.Cancelled;
        await apartmentRequestRepository.UpdateApartmentRequestAsync(originalRecord, apartmentRequest,
            currentUser.Email);

        return ServiceResult<string>.InfoResult(StatusCodes.Status200OK, "Apartment request cancelled successfully");
    }

    public async Task<ServiceResult<string>> LeaveApartmentRequest(int apartmentId,
        LeaveDismissRequestDto leaveRequestDto)
    {
        leaveRequestDto.RequestDate = GetValidRequestDate(leaveRequestDto.RequestDate);

        var currentUser = userContext.GetCurrentUser();

        logger.LogInformation("Requesting to leave apartment with Id = {Id}", apartmentId);

        var apartment = await apartmentRepository.GetApartmentByIdAsync(apartmentId) ??
                        throw new NotFoundException(nameof(Apartment), apartmentId.ToString());

        var tenant = await userRepository.GetTenantByApartmentId(apartmentId) ??
                     throw new NotFoundException($"Apartment with id = {apartmentId} doesn't have a tenant");

        if (!authorizationManager.AuthorizeApartmentRequest(currentUser, ResourceOperation.Create,
                ApartmentRequestType.Leave) ||
            currentUser.Id != tenant.Id)
            throw new ForbiddenException();

        return await leaveRequestHandler.SendLeaveRequest(currentUser, apartment, leaveRequestDto);
    }

    public async Task<ServiceResult<string>> ApproveRejectApartmentRequest(int requestId, string action)
    {
        var requestAction = CoreUtilities.ValidateEnum<RequestAction>(action);

        var currentUser = userContext.GetCurrentUser();

        logger.LogInformation("Approve/Reject apartment request with Id = {Id}", requestId);

        var apartmentRequest = await apartmentRequestRepository.GetApartmentRequestByIdAsync(requestId) ??
                               throw new NotFoundException("Apartment Request", requestId.ToString());

        var apartmentRequestType = CoreUtilities.ValidateEnum<ApartmentRequestType>(apartmentRequest.RequestType);

        if (!authorizationManager.AuthorizeApartmentRequest(currentUser, ResourceOperation.ApproveReject,
                apartmentRequestType, apartmentRequest)) throw new ForbiddenException();

        return apartmentRequestType switch
        {
            ApartmentRequestType.Rent => await rentRequestHandler.ApproveReject(currentUser, apartmentRequest,
                requestAction),
            ApartmentRequestType.Leave => await leaveRequestHandler.ApproveReject(currentUser, apartmentRequest,
                requestAction),
            _ => ServiceResult<string>.ErrorResult(StatusCodes.Status400BadRequest, "Invalid apartment request type.")
        };
    }

    private DateOnly GetValidRequestDate(DateOnly? requestDate)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return requestDate.HasValue && requestDate.Value > today ? requestDate.Value : today;
    }
}