using Apartments.Application.Common;
using Apartments.Application.Dtos.ApartmentRequestDtos;
using Apartments.Application.IServices;
using Apartments.Domain.QueryFilters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apartments.API.Controllers;

[ApiController]
[Route("api/requests")]
[Authorize]
public class ApartmentRequestController(IApartmentRequestService apartmentRequestService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetApartmentRequests([FromQuery] ApartmentRequestQueryFilter apartmentRequestQueryFilter)
    {
        var result = await apartmentRequestService.GetApartmentRequests(apartmentRequestQueryFilter);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetApartmentRequestById([FromRoute] int id)
    {
        var result = await apartmentRequestService.GetApartmentRequestById(id);
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new ResultDetails(result.Message));
        }

        return Ok(result.Data);
    }

    [HttpPost("{id:int}")]
    public async Task<IActionResult> ApproveRejectApartmentRequest([FromRoute] int id, [FromQuery] string action)
    {
        var result = await apartmentRequestService.ApproveRejectApartmentRequest(id, action);

        return StatusCode(result.StatusCode, new ResultDetails(result.Message));
    }
    [HttpPatch("{id:int}")]
    public async Task<IActionResult> UpdateApartmentRequest([FromRoute] int id, [FromBody] UpdateApartmentRequestDto updateApartmentRequestDto)
    {
        var result = await apartmentRequestService.UpdateApartmentRequest(id, updateApartmentRequestDto);
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new ResultDetails(result.Message));
        }

        return Ok(result.Data);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> CancelApartmentRequest([FromRoute] int id)
    {
        var result = await apartmentRequestService.CancelApartmentRequest(id);

        return StatusCode(result.StatusCode, new ResultDetails(result.Message));
    }
}
