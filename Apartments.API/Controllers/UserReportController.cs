using Apartments.Application.Common;
using Apartments.Application.Dtos.UserReportDtos;
using Apartments.Application.IServices;
using Apartments.Domain.QueryFilters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apartments.API.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
public class UserReportController(IUserReportService userReportService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetUserReports([FromQuery] UserReportQueryFilter filter)
    {
        var result = await userReportService.GetUserReportsPaged(filter);

        if (!result.Success) return StatusCode(result.StatusCode, new ResultDetails(result.Message));

        return Ok(result.Data);
    }
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetReportById([FromRoute] int id)
    {
        var result = await userReportService.GetReportById(id);

        if (!result.Success) return StatusCode(result.StatusCode, new ResultDetails(result.Message));

        return Ok(result.Data);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUserReport([FromForm] CreateUserReportDto createReportDto)
    {
        var result = await userReportService.CreateUserReport(createReportDto);

        return StatusCode(result.StatusCode, new ResultDetails(result.Message));
    }

    [HttpPatch("{id:int}")]
    public async Task<IActionResult> UpdateUserReport([FromRoute] int id, [FromForm] UpdateUserReportDto updateReportDto)
    {
        var result = await userReportService.UpdateUserReport(id, updateReportDto);

        return StatusCode(result.StatusCode, new ResultDetails(result.Message));
    }
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteUserReport([FromRoute] int id)
    {
        var result = await userReportService.DeleteUserReport(id);

        return StatusCode(result.StatusCode, new ResultDetails(result.Message));
    }
}
