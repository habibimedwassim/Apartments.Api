using Apartments.Application.Common;
using Apartments.Application.Dtos.ApartmentRequestDtos;
using Apartments.Application.IServices;
using Apartments.Application.Services.ApartmentRequestHandlers;
using Apartments.Application.Utilities;
using Apartments.Domain.Common;
using Apartments.Domain.Entities;
using Apartments.Domain.Exceptions;
using Apartments.Domain.IRepositories;
using Apartments.Domain.QueryFilters;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Apartments.Application.Services;

public class ApartmentRequestService(
    ILogger<ApartmentRequestService> logger,
    IMapper mapper,
    IUserContext userContext,
    IAuthorizationManager authorizationManager,
    IDismissRequestHandler dismissTenantHandler,
    ILeaveRequestHandler leaveRequestHandler,
    IRentRequestHandler rentRequestHandler,
    IUserRepository userRepository,
    IApartmentRepository apartmentRepository,
    IApartmentRequestRepository apartmentRequestRepository,
    IRentTransactionRepository rentTransactionRepository)
    : IApartmentRequestService
{
    public async Task<ServiceResult<string>> ApplyForApartment(int apartmentId)
    {
        var currentUser = userContext.GetCurrentUser();

        if (!authorizationManager.AuthorizeApartmentRequest(currentUser, ResourceOperation.Create,
                ApartmentRequestType.Rent)) throw new ForbiddenException("Not authorized to apply for an apartment");

        logger.LogInformation("Applying for Apartment with ID = {ApartmentId}", apartmentId);

        var existingApartment = await apartmentRepository.GetApartmentByIdAsync(apartmentId) ??
                                throw new NotFoundException(nameof(Apartment), apartmentId.ToString());

        return await rentRequestHandler.SendRentRequest(currentUser, existingApartment);
    }

    public async Task<PagedResult<ApartmentRequestDto>> GetApartmentRequests(
        ApartmentRequestQueryFilter apartmentRequestQueryFilter)
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