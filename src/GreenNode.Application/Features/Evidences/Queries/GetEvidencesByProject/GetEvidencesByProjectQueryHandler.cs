using GreenNode.Application.Common.Exceptions;
using GreenNode.Application.Common.Interfaces;
using GreenNode.Application.Features.Evidences.DTOs;
using GreenNode.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GreenNode.Application.Features.Evidences.Queries.GetEvidencesByProject;

public class GetEvidencesByProjectQueryHandler
    : IRequestHandler<GetEvidencesByProjectQuery, IReadOnlyList<EvidenceDto>>
{
    private readonly IApplicationDbContext _context;

    public GetEvidencesByProjectQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<EvidenceDto>> Handle(
        GetEvidencesByProjectQuery request, CancellationToken ct)
    {
        Project project = await _context.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, ct)
            ?? throw new NotFoundException(nameof(Project), request.ProjectId);

        if (request.CompanyId.HasValue && project.CompanyId != request.CompanyId.Value)
            throw new ForbiddenException("No tienes acceso a las evidencias de este proyecto.");

        List<Evidence> evidences = await _context.Evidences
            .AsNoTracking()
            .Where(e => e.ProjectId == request.ProjectId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(ct);

        return evidences.Select(e => new EvidenceDto(
            e.Id, e.ProjectId, e.PhotoUrl, e.AiValidated,
            e.AiConfidence, e.AiEstimatedTrees, e.AiObservations, e.CreatedAt
        )).ToList().AsReadOnly();
    }
}
