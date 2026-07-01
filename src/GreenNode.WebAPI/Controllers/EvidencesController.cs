using System.Security.Claims;
using GreenNode.Application.Features.Evidences.Commands.UploadEvidence;
using GreenNode.Application.Features.Evidences.DTOs;
using GreenNode.Application.Features.Evidences.Queries.GetEvidencesByProject;
using GreenNode.WebAPI.Hubs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace GreenNode.WebAPI.Controllers;

[ApiController]
[Route("api/projects/{projectId:guid}/evidences")]
[Authorize]
public class EvidencesController(ISender sender, IHubContext<DashboardHub> hubContext) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload(
        [FromRoute] Guid projectId,
        IFormFile file,
        CancellationToken ct)
    {
        Guid? callerCompanyId = User.IsInRole("Admin") ? null : GetCompanyId();

        using Stream stream = file.OpenReadStream();
        EvidenceDto result = await sender.Send(
            new UploadEvidenceCommand(projectId, callerCompanyId, stream, file.FileName, file.ContentType), ct);

        await hubContext.Clients.Group("Admins")
            .SendAsync(DashboardHubMethods.EvidenceUploaded, result, ct);

        return Ok(new { data = result, message = "Evidencia subida correctamente." });
    }

    [HttpGet]
    public async Task<IActionResult> GetByProject([FromRoute] Guid projectId, CancellationToken ct)
    {
        Guid? companyId = User.IsInRole("Admin") ? null : GetCompanyId();
        IReadOnlyList<EvidenceDto> result = await sender.Send(
            new GetEvidencesByProjectQuery(projectId, companyId), ct);
        return Ok(new { data = result, message = "Evidencias obtenidas." });
    }

    private Guid GetCompanyId() =>
        Guid.Parse(User.FindFirstValue("companyId")!);
}
