using Apartments.Application.Common;
using Apartments.Application.Dtos.ApartmentRequestDtos;
using Apartments.Application.Dtos.UserDtos;
using Apartments.Domain.Common;
using Apartments.Domain.QueryFilters;

namespace Apartments.Application.IServices;

public interface IApartmentRequestService
{
    Task<PagedResult<ApartmentRequestDto>>
        GetApartmentRequestsPaged(ApartmentRequestPagedQueryFilter apartmentRequestQueryFilter);

    Task<ServiceResult<ApartmentRequestDto>> GetApartmentRequestById(int requestId);

    Task<ServiceResult<ApartmentRequestDto>> UpdateApartmentRequest(int requestId,
        UpdateApartmentRequestDto updateApartmentRequestDto);

    Task<ServiceResult<string>> ApplyForApartment(int apartmentId);
    Task<ServiceResult<string>> DismissTenantFromApartment(int apartmentId, LeaveDismissRequestDto dismissRequestDto);
    Task<ServiceResult<string>> DismissTenantById(int userId, LeaveDismissRequestDto dismissRequestDto);
    Task<ServiceResult<string>> CancelApartmentRequest(int requestId);
    Task<ServiceResult<string>> LeaveApartmentRequest(int apartmentId, LeaveDismissRequestDto leaveRequestDto);
    Task<ServiceResult<string>> ApproveRejectApartmentRequest(int requestId, string action);
    Task<ServiceResult<IEnumerable<ApartmentRequestDto>>> GetApartmentRequests(ApartmentRequestQueryFilter apartmentRequestQueryFilter);
    Task<ServiceResult<UserDto>> GetTenantByRequestId(int id);
    Task<ServiceResult<string>> ScheduleMeeting(int id, MeetingDateDto meetingDate);
}