using Microsoft.AspNetCore.Mvc;
using RenTN.Application.DTOs.RentalRequestDTOs;
using RenTN.Application.Services.RentalRequestService;
using RenTN.Domain.Common;

namespace RenTN.API.Controllers;

[ApiController]
[Route("api/rental-requests")]
public class RentalRequestController(IRentalRequestService _rentalRequestService) : ControllerBase
{
    [HttpGet("sent")]
    public async Task<IActionResult> GetSentRentalRequests()
    {
        var requests = await _rentalRequestService.GetAllRentalRequests(RentalRequestType.Sent);
        return Ok(requests);
    }

    [HttpGet("received")]
    public async Task<IActionResult> GetReceivedRentalRequests()
    {
        var requests = await _rentalRequestService.GetAllRentalRequests(RentalRequestType.Received);
        return Ok(requests);
    }

    [HttpPost]
    public async Task<IActionResult> SendRentalRequest([FromBody] CreateRentalRequestDTO createRentalRequestDTO)
    {
        var result = await _rentalRequestService.SendRentalRequest(createRentalRequestDTO);

        if (!result.Success)
        {
            return Utilities.ApiResponse.Error(result);
        }

        return Ok(result.Message);
    }
}
