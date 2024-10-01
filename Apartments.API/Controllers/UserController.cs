using Apartments.Application.Common;
using Apartments.Application.Dtos.AdminDtos;
using Apartments.Application.Dtos.ApartmentRequestDtos;
using Apartments.Application.Dtos.UserDtos;
using Apartments.Application.IServices;
using Apartments.Domain.Common;
using Apartments.Domain.QueryFilters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apartments.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UserController(
    IUserService userService,
    IApartmentService apartmentService,
    IRentTransactionService rentTransactionService,
    IApartmentRequestService apartmentRequestService)
    : ControllerBase
{
    [HttpGet("me")]
    public async Task<IActionResult> GetUserProfile()
    {
        var user = await userService.GetUserProfile();
        return Ok(user.Data);
    }

    [HttpGet("me/apartments")]
    public async Task<IActionResult> GetOwnedApartments()
    {
        var result = await apartmentService.GetOwnedApartments();
        if (!result.Success) return StatusCode(result.StatusCode, new ResultDetails(result.Message));

        return Ok(result.Data);
    }

    [HttpGet("me/requests")]
    public async Task<IActionResult> GetApartmentRequests(
        [FromQuery] ApartmentRequestQueryFilter apartmentRequestQueryFilter)
    {
        var result = await apartmentRequestService.GetApartmentRequests(apartmentRequestQueryFilter);
        return Ok(result);
    }

    [HttpGet("me/transactions")]
    public async Task<IActionResult> GetRentTransactions()
    {
        var result = await rentTransactionService.GetRentTransactions();
        if (!result.Success) return StatusCode(result.StatusCode, new ResultDetails(result.Message));

        return Ok(result.Data);
    }

    [HttpGet("{id:int}/apartments")]
    public async Task<IActionResult> GetOwnedApartments([FromRoute] int id)
    {
        var result = await apartmentService.GetOwnedApartments(id);
        if (!result.Success) return StatusCode(result.StatusCode, new ResultDetails(result.Message));

        return Ok(result.Data);
    }

    [HttpGet("{id:int}/transactions")]
    public async Task<IActionResult> GetRentTransactions([FromRoute] int id)
    {
        var result = await rentTransactionService.GetRentTransactions(id);
        if (!result.Success) return StatusCode(result.StatusCode, new ResultDetails(result.Message));

        return Ok(result.Data);
    }

    [HttpPatch("me")]
    public async Task<IActionResult> UpdateUserDetails([FromBody] UpdateUserDto updateUserDto)
    {
        var result = await userService.UpdateUserDetails(updateUserDto);
        return StatusCode(result.StatusCode, new ResultDetails(result.Message));
    }

    [HttpPost("{id:int}/dismiss")]
    public async Task<IActionResult> DismissTenantById([FromRoute] int id,
        [FromBody] LeaveDismissRequestDto dismissReasonDto)
    {
        var result = await apartmentRequestService.DismissTenantById(id, dismissReasonDto);

        return StatusCode(result.StatusCode, new ResultDetails(result.Message));
    }
}