using System.Security.Claims;
using GreenNode.Application.Features.Advisor.Commands.AskAdvisor;
using GreenNode.Application.Features.Advisor.Commands.AskAdvisor.DTOs;
using GreenNode.WebAPI.Contracts.Advisor;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenNode.WebAPI.Controllers;

[ApiController]
[Route("api/advisor")]
[Authorize(Roles = "Company")]
public class AdvisorController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Ask(AskAdvisorRequest request, CancellationToken ct)
    {
        Guid companyId = Guid.Parse(User.FindFirstValue("companyId")!);
        AdvisorResponseDto result = await sender.Send(
            new AskAdvisorCommand(companyId, request.Question), ct);
        return Ok(new { data = result, message = "Respuesta del asesor obtenida." });
    }
}
