using Apartments.Application.Common;
using Apartments.Application.Dtos.ApartmentRequestDtos;
using Apartments.Domain.Common;
using Apartments.Domain.QueryFilters;

namespace Apartments.Application.IServices;

public interface IApartmentRequestService
{
    Task<PagedResult<ApartmentRequestDto>> GetApartmentRequests(ApartmentRequestQueryFilter apartmentRequestQueryFilter);
    Task<ServiceResult<ApartmentRequestDto>> GetApartmentRequestById(int requestId);
    Task<ServiceResult<ApartmentRequestDto>> UpdateApartmentRequest(int requestId, UpdateApartmentRequestDto updateApartmentRequestDto);
    Task<ServiceResult<string>> ApplyForApartment(int apartmentId);
    Task<ServiceResult<string>> DismissTenantFromApartment(int apartmentId, LeaveDismissReasonDto dismissReasonDto);
    Task<ServiceResult<string>> DismissTenantById(int userId, LeaveDismissReasonDto dismissReasonDto);
    Task<ServiceResult<string>> CancelApartmentRequest(int requestId);
    Task<ServiceResult<string>> LeaveApartmentRequest(int apartmentId, LeaveDismissReasonDto leaveReasonDto);
    Task<ServiceResult<string>> ApproveRejectApartmentRequest(int requestId, string action);
}

