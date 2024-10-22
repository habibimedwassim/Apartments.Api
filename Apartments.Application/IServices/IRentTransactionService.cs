using Apartments.Application.Common;
using Apartments.Application.Dtos.RentTransactionDtos;
using Apartments.Domain.Common;
using Apartments.Domain.QueryFilters;

namespace Apartments.Application.IServices;

public interface IRentTransactionService
{
    Task<ServiceResult<RentTransactionDto>> GetRentTransactionById(int id);
    //Task<ServiceResult<List<RentTransactionDto>>> GetRentTransactions(int? id = null);
    Task<ServiceResult<string>> CreateRentTransactionForApartment(int id);
    Task<ServiceResult<string>> UpdateRentTransaction(int id, string action);
    Task<PagedResult<RentTransactionDto>> GetRentTransactionsPaged(RentTransactionQueryFilter filter);
}