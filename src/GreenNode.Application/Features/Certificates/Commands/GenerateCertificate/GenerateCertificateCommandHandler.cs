using GreenNode.Application.Common.Exceptions;
using GreenNode.Application.Common.Interfaces;
using GreenNode.Application.Common.Models;
using GreenNode.Domain.Entities;
using GreenNode.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GreenNode.Application.Features.Certificates.Commands.GenerateCertificate;

public class GenerateCertificateCommandHandler : IRequestHandler<GenerateCertificateCommand, string>
{
    private readonly IApplicationDbContext _context;
    private readonly ICertificateService _certificateService;
    private readonly IEmailService _emailService;
    private readonly ILogger<GenerateCertificateCommandHandler> _logger;

    public GenerateCertificateCommandHandler(
        IApplicationDbContext context,
        ICertificateService certificateService,
        IEmailService emailService,
        ILogger<GenerateCertificateCommandHandler> logger)
    {
        _context = context;
        _certificateService = certificateService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<string> Handle(GenerateCertificateCommand request, CancellationToken ct)
    {
        Project project = await _context.Projects
            .Include(p => p.Company)
            .Include(p => p.Territory)
            .Include(p => p.Certificate)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, ct)
            ?? throw new NotFoundException(nameof(Project), request.ProjectId);

        if (project.Status != ProjectStatus.Completed)
            throw new AppValidationException(
                "Solo se puede generar certificado para proyectos completados.");

        if (project.Certificate is not null)
            throw new AppValidationException(
                "Este proyecto ya tiene un certificado generado.");

        decimal co2 = project.ValidatedTrees * 21.77m;
        string verificationCode = $"GN-{project.Id.ToString()[..8].ToUpper()}";

        CertificateData data = new(
            project.Company.Name, project.Territory.Name,
            project.ValidatedTrees, co2, verificationCode, DateTime.UtcNow);

        string pdfKey = await _certificateService.GenerateCertificateAsync(data, ct);

        Certificate certificate = new(
            project.Id, project.Company.Name, project.Territory.Name,
            project.ValidatedTrees, co2, pdfKey, verificationCode);
        _context.Certificates.Add(certificate);
        project.SetCertificateUrl(pdfKey);
        await _context.SaveChangesAsync(ct);

        _ = _emailService.SendCertificateReadyEmailAsync(
            project.Company.Email, project.Company.Name, CancellationToken.None);

        _logger.LogInformation("Certificado generado para Project {ProjectId}", project.Id);

        return pdfKey;
    }
}
