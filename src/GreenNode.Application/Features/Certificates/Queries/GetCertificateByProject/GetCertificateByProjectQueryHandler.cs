using GreenNode.Application.Common.Exceptions;
using GreenNode.Application.Common.Interfaces;
using GreenNode.Application.Features.Certificates.DTOs;
using GreenNode.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GreenNode.Application.Features.Certificates.Queries.GetCertificateByProject;

// Implementado por Juan como fallback temporal — Cami debe revisar antes de merge a develop
public class GetCertificateByProjectQueryHandler : IRequestHandler<GetCertificateByProjectQuery, CertificateDto>
{
    private readonly IApplicationDbContext _context;

    public GetCertificateByProjectQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CertificateDto> Handle(GetCertificateByProjectQuery request, CancellationToken ct)
    {
        Certificate certificate = await _context.Certificates
            .AsNoTracking()
            .Include(c => c.Project)
            .FirstOrDefaultAsync(c => c.ProjectId == request.ProjectId, ct)
            ?? throw new NotFoundException("Certificate", request.ProjectId);

        if (request.CallerCompanyId.HasValue && certificate.Project.CompanyId != request.CallerCompanyId.Value)
            throw new ForbiddenException("No tienes acceso a este certificado.");

        return new CertificateDto(
            certificate.Id, certificate.ProjectId,
            certificate.CompanyName, certificate.TerritoryName,
            certificate.NumberOfTrees, certificate.Co2EstimatedKg,
            certificate.PdfUrl, certificate.VerificationCode,
            certificate.IssuedAt, certificate.CreatedAt);
    }
}
