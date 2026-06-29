using GreenNode.Application.Common.Exceptions;
using GreenNode.Application.Common.Interfaces;
using GreenNode.Application.Features.Evidences.DTOs;
using GreenNode.Application.Features.Projects.DTOs;
using GreenNode.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GreenNode.Application.Features.Projects.Queries.GetProjectById;

public class GetProjectByIdQueryHandler : IRequestHandler<GetProjectByIdQuery, ProjectDetailDto>
{
    private readonly IApplicationDbContext _context;

    public GetProjectByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProjectDetailDto> Handle(GetProjectByIdQuery request, CancellationToken ct)
    {
        Project project = await _context.Projects
            .AsNoTracking()
            .Include(p => p.Company)
            .Include(p => p.Territory)
            .Include(p => p.Evidences)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, ct)
            ?? throw new NotFoundException(nameof(Project), request.ProjectId);

        if (request.CompanyId.HasValue && project.CompanyId != request.CompanyId.Value)
            throw new ForbiddenException("No tienes acceso a este proyecto.");

        IReadOnlyList<EvidenceDto> evidenceDtos = project.Evidences
            .OrderByDescending(e => e.CreatedAt)
            .Select(e => new EvidenceDto(
                e.Id, e.ProjectId, e.PhotoUrl, e.AiValidated,
                e.AiConfidence, e.AiEstimatedTrees,
                e.AiObservations, e.CreatedAt))
            .ToList()
            .AsReadOnly();

        return new ProjectDetailDto(
            project.Id, project.CompanyId, project.Company.Name,
            project.QuoteId, project.Territory.Name,
            project.Territory.Department, project.Territory.Municipality,
            project.Sector, project.NumberOfTrees, project.ValidatedTrees,
            project.ProgressPercentage, project.Status.ToString(),
            project.CertificateUrl, evidenceDtos, project.CreatedAt);
    }
}
