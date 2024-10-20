using Apartments.Application.Common;
using Apartments.Application.Dtos.AdminDtos;
using Apartments.Application.Dtos.ApartmentRequestDtos;
using Apartments.Application.Dtos.AuthDtos;
using Apartments.Application.Dtos.UserDtos;
using Apartments.Application.IServices;
using Apartments.Application.Services;
using Apartments.Domain.Common;
using Apartments.Domain.Entities;
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
    IApartmentRequestService apartmentRequestService,
    IDashboardService dashboardService)
    : ControllerBase
{
    [HttpGet("me")]
    public async Task<IActionResult> GetUserProfile()
    {
        var user = await userService.GetUserProfile();
        return Ok(user.Data);
    }

    [HttpPatch("me")]
    public async Task<IActionResult> UpdateUserDetails([FromForm] UpdateUserDto updateUserDto)
    {
        var result = await userService.UpdateUserDetails(updateUserDto);
        if (!result.Success) return StatusCode(result.StatusCode, new ResultDetails(result.Message));

        return Ok(result.Data);
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> UpdateUserPassword([FromBody] ChangePasswordDto changePasswordDto)
    {
        var result = await userService.UpdateUserPassword(changePasswordDto);
        return StatusCode(result.StatusCode, new ResultDetails(result.Message));
    }

    [HttpPost("change-email")]
    public async Task<IActionResult> UpdateUserEmail([FromBody] EmailDto changeEmailDto)
    {
        var result = await userService.UpdateUserEmail(changeEmailDto);
        return StatusCode(result.StatusCode, new ResultDetails(result.Message));
    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyNewEmailDto verifyNewEmailDTO)
    {
        var result = await userService.VerifyEmailAsync(verifyNewEmailDTO);

        return StatusCode(result.StatusCode, new ResultDetails(result.Message));
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
        return Ok(result.Data);
    }
    [HttpGet("me/tenants")]
    public async Task<IActionResult> GetMyTenants()
    {
        var result = await userService.GetOwnerTenants();
        return Ok(result.Data);
    }
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetMyTenants([FromRoute] int id)
    {
        var result = await userService.GetUserById(id);
        return Ok(result.Data);
    }
    [HttpGet("me/requests-paged")]
    public async Task<IActionResult> GetApartmentRequestsPaged(
        [FromQuery] ApartmentRequestPagedQueryFilter apartmentRequestQueryFilter)
    {
        var result = await apartmentRequestService.GetApartmentRequestsPaged(apartmentRequestQueryFilter);
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
    //[HttpGet("apartments/{apartmentId:int}")]
    //public async Task<IActionResult> GetOwnedApartmentById([FromRoute] int userId, [FromRoute] int apartmentId)
    //{
    //    var result = await apartmentService.GetOwnedApartments(id);
    //    if (!result.Success) return StatusCode(result.StatusCode, new ResultDetails(result.Message));

    //    return Ok(result.Data);
    //}

    [HttpGet("{id:int}/transactions")]
    public async Task<IActionResult> GetRentTransactions([FromRoute] int id)
    {
        var result = await rentTransactionService.GetRentTransactions(id);
        if (!result.Success) return StatusCode(result.StatusCode, new ResultDetails(result.Message));

        return Ok(result.Data);
    }

    [HttpPost("{id:int}/dismiss")]
    public async Task<IActionResult> DismissTenantById([FromRoute] int id,
        [FromBody] LeaveDismissRequestDto dismissReasonDto)
    {
        var result = await apartmentRequestService.DismissTenantById(id, dismissReasonDto);

        return StatusCode(result.StatusCode, new ResultDetails(result.Message));
    }

    [HttpGet("me/dashboard")]
    public async Task<IActionResult> GetOwnerDashboard()
    {
        var result = await dashboardService.GetOwnerDashboard();

        if (!result.Success) return StatusCode(result.StatusCode, new ResultDetails(result.Message));

        return Ok(result.Data);
    }
}