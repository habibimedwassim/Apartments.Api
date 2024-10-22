using Apartments.Application.Common;
using Apartments.Application.Dtos.ApartmentDtos;
using Apartments.Application.Dtos.RentTransactionDtos;
using Apartments.Application.IServices;
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

public class RentTransactionService(
    ILogger<RentTransactionService> logger,
    IMapper mapper,
    IUserContext userContext,
    IAuthorizationManager authorizationManager,
    IUserRepository userRepository,
    IApartmentRepository apartmentRepository,
    IRentTransactionRepository rentTransactionRepository)
    : IRentTransactionService
{
    public async Task<ServiceResult<RentTransactionDto>> GetRentTransactionById(int id)
    {
        logger.LogInformation("Retrieving Rent Transaction with Id = {id}", id);

        var rentTransaction = await rentTransactionRepository.GetRentTransactionByIdAsync(id) ??
                              throw new NotFoundException(nameof(RentTransaction), id.ToString());

        var rentTransactionDto = mapper.Map<RentTransactionDto>(rentTransaction);

        return ServiceResult<RentTransactionDto>.SuccessResult(rentTransactionDto);
    }

    //public async Task<ServiceResult<List<RentTransactionDto>>> GetRentTransactions(int? id = null)
    //{
    //    var currentUser = userContext.GetCurrentUser();
    //    var sysId = id ?? currentUser.SysId;

    //    logger.LogInformation("Retrieving User with Id = {id} rent transactions", sysId);

    //    var user = await userRepository.GetBySysIdAsync(sysId) ??
    //               throw new NotFoundException("User not found");

    //    var rentTransactions = await rentTransactionRepository.GetRentTransactionsForUserAsync(user.Id, user.Role);

    //    // Pass the current user's role to the mapping configuration
    //var rentTransactionsDto = mapper.Map<List<RentTransactionDto>>(rentTransactions, opt =>
    //{
    //    opt.Items["UserRole"] = user.Role;
    //});

    //    return ServiceResult<List<RentTransactionDto>>.SuccessResult(rentTransactionsDto);
    //}
    public async Task<PagedResult<RentTransactionDto>> GetRentTransactionsPaged(RentTransactionQueryFilter filter)
    {
        var currentUser = userContext.GetCurrentUser();
        var sysId = filter.userId ?? currentUser.SysId;

        logger.LogInformation("Retrieving User with Id = {id} rent transactions", sysId);

        var user = await userRepository.GetBySysIdAsync(sysId) ??
                   throw new NotFoundException("User not found");

        var pagedModel = await rentTransactionRepository.GetRentTransactionsPagedAsync(filter, user.Id, user.Role);

        var rentTransactionsDto = mapper.Map<IEnumerable<RentTransactionDto>>(pagedModel.Data, opt => 
        {
            opt.Items["UserRole"] = user.Role;
        });

        var result = new PagedResult<RentTransactionDto>(rentTransactionsDto, pagedModel.DataCount, filter.PageNumber);

        return result;
    }

    public async Task<ServiceResult<string>> CreateRentTransactionForApartment(int id)
    {
        logger.LogInformation("Paying for apartment with Id = {id}", id);

        var currentUser = userContext.GetCurrentUser();

        var apartment = await apartmentRepository.GetApartmentByIdAsync(id) ??
                        throw new NotFoundException(nameof(Apartment), id.ToString());

        var user = await userRepository.GetByUserIdAsync(currentUser.Id) ??
                   throw new NotFoundException("User not found");

        if (apartment.TenantId == null || apartment.TenantId != user.Id)
            return ServiceResult<string>.ErrorResult(StatusCodes.Status400BadRequest,
                "User can't pay for this apartment");

        if (!authorizationManager.AuthorizeRentTransaction(currentUser, ResourceOperation.Create))
            throw new ForbiddenException("User can't pay for this apartment");

        var latestTransaction = await rentTransactionRepository.GetLatestRentTransactionAsync(apartment.Id, user.Id) ??
                                throw new NotFoundException("The owner needs to accept your request first");

        var dateFrom = latestTransaction.DateFrom;
        var dateTo = latestTransaction.DateTo.HasValue ? 
                     latestTransaction.DateTo.Value 
                     : DateOnly.FromDateTime(DateTime.UtcNow);

        var rentTransaction = new RentTransaction
        {
            OwnerId = apartment.OwnerId,
            TenantId = currentUser.Id,
            ApartmentId = id,
            DateFrom = dateTo,
            DateTo = dateTo.AddMonths(1),
            RentAmount = apartment.RentAmount
        };

        await rentTransactionRepository.AddRentTransactionAsync(rentTransaction);

        return ServiceResult<string>.InfoResult(StatusCodes.Status200OK, "Transaction created successfully!");
    }

    public async Task<ServiceResult<string>> UpdateRentTransaction(int id, string action)
    {
        var requestAction = CoreUtilities.ValidateEnum<PaymentRequestAction>(action);

        var currentUser = userContext.GetCurrentUser();

        var rentTransaction = await rentTransactionRepository.GetRentTransactionByIdAsync(id) ??
                              throw new NotFoundException(nameof(RentTransaction), id.ToString());

        if (!authorizationManager.AuthorizeRentTransaction(currentUser, ResourceOperation.Update, rentTransaction))
            throw new ForbiddenException();

        var originalRecord = mapper.Map<RentTransaction>(rentTransaction);

        rentTransaction.Status = requestAction switch
        {
            PaymentRequestAction.Accept => RequestStatus.Paid,
            PaymentRequestAction.Late => RequestStatus.Late,
            PaymentRequestAction.Reset => RequestStatus.Pending,
            PaymentRequestAction.Cancel => await HandleCancelRentTransactionAsync(rentTransaction, currentUser),
            _ => rentTransaction.Status
        };

        await rentTransactionRepository.UpdateRentTransactionAsync(originalRecord, rentTransaction, currentUser.Email);

        return ServiceResult<string>.InfoResult(StatusCodes.Status200OK, "Transaction updated successfully!");
    }

    private async Task<string> HandleCancelRentTransactionAsync(RentTransaction rentTransaction,
        CurrentUser currentUser)
    {
        await rentTransactionRepository.DeleteRentTransactionAsync(rentTransaction, currentUser.Email);
        return RequestStatus.Cancelled;
    }
}