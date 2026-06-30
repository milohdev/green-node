using GreenNode.Application.Features.Evidences.DTOs;
using MediatR;

namespace GreenNode.Application.Features.Evidences.Commands.UploadEvidence;

// Cambio de firma propuesto desde carril WebAPI (Juan) para desbloquear Admin uploads.
// CallerCompanyId == null significa que el caller es Admin (sin restricción de empresa).
// CallerCompanyId con valor significa que el caller es Company (se valida pertenencia).
// Debe ser revisado y aprobado por Cami antes de mergear a develop.
public record UploadEvidenceCommand(
    Guid ProjectId,
    Guid? CallerCompanyId,
    Stream FileStream,
    string FileName,
    string ContentType
) : IRequest<EvidenceDto>;
