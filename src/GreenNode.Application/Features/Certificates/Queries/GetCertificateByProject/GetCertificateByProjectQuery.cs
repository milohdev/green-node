using GreenNode.Application.Features.Certificates.DTOs;
using MediatR;

namespace GreenNode.Application.Features.Certificates.Queries.GetCertificateByProject;

// Implementado por Juan como fallback temporal — Cami debe revisar antes de merge a develop
public record GetCertificateByProjectQuery(
    Guid ProjectId,
    Guid? CallerCompanyId
) : IRequest<CertificateDto>;
