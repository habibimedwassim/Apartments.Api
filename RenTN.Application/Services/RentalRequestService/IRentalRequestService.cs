using RenTN.Application.DTOs.RentalRequestDTOs;
using RenTN.Application.Utilities;
using RenTN.Domain.Common;

namespace RenTN.Application.Services.RentalRequestService;

public interface IRentalRequestService
{
    Task<IEnumerable<RentalRequestDTO>> GetAllRentalRequests(RentalRequestType sent);
    Task<ApplicationResponse> SendRentalRequest(CreateRentalRequestDTO createRentalRequestDTO);
}
