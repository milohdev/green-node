using System.Security.Claims;
using GreenNode.Application.Features.Certificates.Commands.GenerateCertificate;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenNode.WebAPI.Controllers;

[ApiController]
[Route("api/certificates")]
[Authorize]
public class CertificatesController(ISender sender) : ControllerBase
{
    // GetCertificateByProjectQuery es stub en Application — pendiente de implementación por Cami
    [HttpGet("{projectId:guid}")]
    public IActionResult GetByProject(Guid projectId)
    {
        return StatusCode(StatusCodes.Status501NotImplemented,
            new { message = "Pendiente: GetCertificateByProjectQuery no está implementado aún en Application." });
    }

    [HttpPost("{projectId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Generate(Guid projectId, CancellationToken ct)
    {
        string pdfUrl = await sender.Send(new GenerateCertificateCommand(projectId), ct);
        return Ok(new { data = new { url = pdfUrl }, message = "Certificado generado exitosamente." });
    }
}
