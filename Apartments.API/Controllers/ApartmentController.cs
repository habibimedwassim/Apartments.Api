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

        return StatusCode(result.StatusCode, new ResultDetails(result.Message));
    }

    [HttpPatch("{id:int}")]
    public async Task<IActionResult> UpdateApartment([FromRoute] int id,
        [FromBody] UpdateApartmentDto updateApartmentDto)
    {
        var result = await apartmentService.UpdateApartment(id, updateApartmentDto);

        return StatusCode(result.StatusCode, new ResultDetails(result.Message));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteRestoreApartment([FromRoute] int id, [FromQuery] bool permanent = false)
    {
        var result = await apartmentService.DeleteApartment(id, permanent);

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

    [HttpPost("bookmarked")]
    public async Task<IActionResult> GetBookmarkedApartments([FromBody] BookmarkedApartmentsDto apartmentsDto)
    {
        var result = await apartmentService.GetBookmarkedApartments(apartmentsDto.ApartmentsIds);

        return Ok(result);
    }
}