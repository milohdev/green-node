using System.Security.Claims;
using GreenNode.Application.Features.Projects.DTOs;
using GreenNode.Application.Features.Projects.Queries.GetProjectById;
using GreenNode.Application.Features.Projects.Queries.GetProjects;
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

    // UpdateProgressCommand es stub en Application — pendiente de implementación por Cami
    [HttpPut("{id:guid}/progress")]
    [Authorize(Roles = "Admin")]
    public IActionResult UpdateProgress(Guid id)
    {
        return StatusCode(StatusCodes.Status501NotImplemented,
            new { message = "Pendiente: UpdateProgressCommand no está implementado aún en Application." });
    }

    private Guid GetCompanyId() =>
        Guid.Parse(User.FindFirstValue("companyId")!);
}
