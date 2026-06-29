using GreenNode.Application.Features.Evidences.DTOs;
using MediatR;

namespace GreenNode.Application.Features.Evidences.Commands.UploadEvidence;

public record UploadEvidenceCommand(
    Guid ProjectId,
    Guid CompanyId,
    Stream FileStream,
    string FileName,
    string ContentType
) : IRequest<EvidenceDto>;
