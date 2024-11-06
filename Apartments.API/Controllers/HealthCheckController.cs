using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Apartments.API.Controllers;
[ApiController]
[Route("api/health")]
public class HealthCheckController(HealthCheckService healthCheckService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetHealthStatus()
    {
        var healthReport = await healthCheckService.CheckHealthAsync();
        var status = healthReport.Status == HealthStatus.Healthy ? "Healthy" : "Unhealthy";

        return Ok(new
        {
            Status = status,
            Details = healthReport.Entries
        });
    }
}
