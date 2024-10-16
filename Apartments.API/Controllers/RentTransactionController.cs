using Apartments.Application.Common;
using Apartments.Application.Dtos.RentTransactionDtos;
using Apartments.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apartments.API.Controllers;

[ApiController]
[Route("api/transactions")]
[Authorize]
public class RentTransactionController(IRentTransactionService rentTransactionService) : ControllerBase
{
    [HttpPost("{id:int}")]
    public async Task<IActionResult> CreateRentTransaction([FromRoute] int id)
    {
        var result = await rentTransactionService.CreateRentTransactionForApartment(id);
        return StatusCode(result.StatusCode, new ResultDetails(result.Message));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetRentTransactionById([FromRoute] int id)
    {
        var result = await rentTransactionService.GetRentTransactionById(id);
        return Ok(result.Data);
    }

    [HttpPatch("{id:int}")]
    public async Task<IActionResult> UpdateRentTransaction([FromRoute] int id, [FromQuery] string action)
    {
        var result = await rentTransactionService.UpdateRentTransaction(id, action);

        return StatusCode(result.StatusCode, new ResultDetails(result.Message));
    }
}