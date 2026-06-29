using GreenNode.Application.Features.Evidences.DTOs;
using MediatR;

namespace GreenNode.Application.Features.Evidences.Queries.GetEvidencesByProject;

public record GetEvidencesByProjectQuery(
    Guid ProjectId,
    Guid? CompanyId
) : IRequest<IReadOnlyList<EvidenceDto>>;
