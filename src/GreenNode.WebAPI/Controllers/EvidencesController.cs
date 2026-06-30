using System.Security.Claims;
using GreenNode.Application.Features.Evidences.Commands.UploadEvidence;
using GreenNode.Application.Features.Evidences.DTOs;
using GreenNode.Application.Features.Evidences.Queries.GetEvidencesByProject;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenNode.WebAPI.Controllers;

[ApiController]
[Route("api/evidences")]
[Authorize]
public class EvidencesController(ISender sender) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload(
        [FromForm] Guid projectId,
        IFormFile file,
        CancellationToken ct)
    {
        // Admin no tiene companyId en JWT; Guid.Empty se pasa como placeholder.
        // El handler lanzará ForbiddenException hasta que Cami actualice la lógica para Admin.
        Guid companyId = Guid.TryParse(User.FindFirstValue("companyId"), out Guid cid)
            ? cid
            : Guid.Empty;

        using Stream stream = file.OpenReadStream();
        EvidenceDto result = await sender.Send(
            new UploadEvidenceCommand(projectId, companyId, stream, file.FileName, file.ContentType), ct);

        return Ok(new { data = result, message = "Evidencia subida correctamente." });
    }

    [HttpGet("{projectId:guid}")]
    public async Task<IActionResult> GetByProject(Guid projectId, CancellationToken ct)
    {
        Guid? companyId = User.IsInRole("Admin") ? null : GetCompanyId();
        IReadOnlyList<EvidenceDto> result = await sender.Send(
            new GetEvidencesByProjectQuery(projectId, companyId), ct);
        return Ok(new { data = result, message = "Evidencias obtenidas." });
    }

    private Guid GetCompanyId() =>
        Guid.Parse(User.FindFirstValue("companyId")!);
}
