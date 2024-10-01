using Apartments.Application.Common;
using Apartments.Application.Dtos.ApartmentDtos;
using Apartments.Application.Dtos.ApartmentRequestDtos;
using Apartments.Application.IServices;
using Apartments.Domain.QueryFilters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apartments.API.Controllers;

[ApiController]
[Route("api/apartments")]
[Authorize]
public class ApartmentController(
    IApartmentService apartmentService,
    IRentTransactionService rentTransactionService,
    IApartmentRequestService apartmentRequestService
) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllApartments([FromQuery] ApartmentQueryFilter apartmentQueryFilter)
    {
        var result = await apartmentService.GetAllApartments(apartmentQueryFilter);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetApartmentById([FromRoute] int id)
    {
        var result = await apartmentService.GetApartmentById(id);
        return Ok(result.Data);
    }

    [HttpPost]
    public async Task<IActionResult> CreateApartment([FromForm] CreateApartmentDto createApartmentDto)
    {
        var result = await apartmentService.CreateApartment(createApartmentDto);

        if (!result.Success) return StatusCode(result.StatusCode, new ResultDetails(result.Message));

        return Ok(result.Data);
    }

    [HttpPatch("{id:int}")]
    public async Task<IActionResult> UpdateApartment([FromRoute] int id,
        [FromBody] UpdateApartmentDto updateApartmentDto)
    {
        var result = await apartmentService.UpdateApartment(id, updateApartmentDto);

        if (!result.Success) return StatusCode(result.StatusCode, new ResultDetails(result.Message));

        return Ok(result.Data);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteApartment([FromRoute] int id)
    {
        var result = await apartmentService.DeleteApartment(id);

        return StatusCode(result.StatusCode, new ResultDetails(result.Message));
    }

    [HttpGet("{id:int}/restore")]
    public async Task<IActionResult> RestoreApartment([FromRoute] int id)
    {
        var result = await apartmentService.RestoreApartment(id);

        return StatusCode(result.StatusCode, new ResultDetails(result.Message));
    }

    [HttpPost("{id:int}/apply")]
    public async Task<IActionResult> ApplyForApartment([FromRoute] int id)
    {
        var result = await apartmentRequestService.ApplyForApartment(id);

        return StatusCode(result.StatusCode, new ResultDetails(result.Message));
    }

    [HttpPost("{id:int}/dismiss")]
    public async Task<IActionResult> DismissTenantFromApartment([FromRoute] int id,
        [FromBody] LeaveDismissRequestDto dismissRequestDto)
    {
        var result = await apartmentRequestService.DismissTenantFromApartment(id, dismissRequestDto);

        return StatusCode(result.StatusCode, new ResultDetails(result.Message));
    }

    [HttpPost("{id:int}/leave")]
    public async Task<IActionResult> LeaveApartmentRequest([FromRoute] int id,
        [FromBody] LeaveDismissRequestDto leaveRequestDto)
    {
        var result = await apartmentRequestService.LeaveApartmentRequest(id, leaveRequestDto);

        return StatusCode(result.StatusCode, new ResultDetails(result.Message));
    }

    [HttpPost("{id:int}/pay")]
    public async Task<IActionResult> PayForApartment([FromRoute] int id)
    {
        var result = await rentTransactionService.CreateRentTransactionForApartment(id);

        return StatusCode(result.StatusCode, new ResultDetails(result.Message));
    }
}