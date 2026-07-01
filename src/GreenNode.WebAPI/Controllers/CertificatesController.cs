using System.Security.Claims;
using GreenNode.Application.Features.Certificates.Commands.GenerateCertificate;
using GreenNode.Application.Features.Certificates.DTOs;
using GreenNode.Application.Features.Certificates.Queries.GetCertificateByProject;
using GreenNode.WebAPI.Hubs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace GreenNode.WebAPI.Controllers;

[ApiController]
[Route("api/certificates")]
[Authorize]
public class CertificatesController(ISender sender, IHubContext<DashboardHub> hubContext) : ControllerBase
{
    [HttpGet("{projectId:guid}")]
    public async Task<IActionResult> GetByProject(Guid projectId, CancellationToken ct)
    {
        Guid? callerCompanyId = User.IsInRole("Admin") ? null : GetCompanyId();
        CertificateDto result = await sender.Send(
            new GetCertificateByProjectQuery(projectId, callerCompanyId), ct);
        return Ok(new { data = result, message = "Certificado obtenido." });
    }

    [HttpPost("{projectId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Generate(Guid projectId, CancellationToken ct)
    {
        string pdfUrl = await sender.Send(new GenerateCertificateCommand(projectId), ct);
        await hubContext.Clients.Group("Admins")
            .SendAsync(DashboardHubMethods.CertificateGenerated, new { url = pdfUrl }, ct);
        return Ok(new { data = new { url = pdfUrl }, message = "Certificado generado exitosamente." });
    }

    private Guid GetCompanyId() =>
        Guid.Parse(User.FindFirstValue("companyId")!);
}
