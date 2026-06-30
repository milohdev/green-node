using System.Security.Claims;
using GreenNode.Application.Features.Projects.Commands.UpdateProgress;
using GreenNode.Application.Features.Projects.DTOs;
using GreenNode.Application.Features.Projects.Queries.GetProjectById;
using GreenNode.Application.Features.Projects.Queries.GetProjects;
using GreenNode.WebAPI.Contracts.Projects;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenNode.WebAPI.Controllers;

[ApiController]
[Route("api/projects")]
[Authorize]
public class ProjectsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? statusFilter, CancellationToken ct)
    {
        Guid? companyId = User.IsInRole("Admin") ? null : GetCompanyId();
        IReadOnlyList<ProjectDto> result = await sender.Send(
            new GetProjectsQuery(companyId, statusFilter), ct);
        return Ok(new { data = result, message = "Proyectos obtenidos." });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        Guid? companyId = User.IsInRole("Admin") ? null : GetCompanyId();
        ProjectDetailDto result = await sender.Send(new GetProjectByIdQuery(id, companyId), ct);
        return Ok(new { data = result, message = "Proyecto obtenido." });
    }

    [HttpPut("{id:guid}/progress")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateProgress(Guid id, [FromBody] UpdateProgressRequest request, CancellationToken ct)
    {
        ProjectDetailDto result = await sender.Send(
            new UpdateProgressCommand(id, request.ValidatedTreeCount), ct);
        return Ok(new { data = result, message = "Progreso actualizado." });
    }

    private Guid GetCompanyId() =>
        Guid.Parse(User.FindFirstValue("companyId")!);
}
