using GreenNode.Application.Common.Models;
using GreenNode.Application.Features.Territories.Queries.GetTerritories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenNode.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TerritoriesController(ISender sender) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await sender.Send(new GetTerritoriesQuery(), ct);
        return Ok(ApiResponse<object>.Success(result, "Territorios obtenidos correctamente."));
    }
}