using GreenNode.Application.Features.Dashboard.Queries.GetDashboardSummary;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenNode.WebAPI.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize(Roles = "Admin")]
public class DashboardController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetSummary(CancellationToken ct)
    {
        DashboardSummaryDto result = await sender.Send(new GetDashboardSummaryQuery(), ct);
        return Ok(new { data = result, message = "Resumen del dashboard obtenido." });
    }
}
