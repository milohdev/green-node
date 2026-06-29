using GreenNode.Application.Common.Interfaces;
using GreenNode.Application.Features.Projects.DTOs;
using GreenNode.Domain.Entities;
using GreenNode.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GreenNode.Application.Features.Projects.Queries.GetProjects;

public class GetProjectsQueryHandler : IRequestHandler<GetProjectsQuery, IReadOnlyList<ProjectDto>>
{
    private readonly IApplicationDbContext _context;

    public GetProjectsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ProjectDto>> Handle(GetProjectsQuery request, CancellationToken ct)
    {
        IQueryable<Project> query = _context.Projects
            .AsNoTracking()
            .Include(p => p.Company)
            .Include(p => p.Territory);

        if (request.CompanyId.HasValue)
            query = query.Where(p => p.CompanyId == request.CompanyId.Value);

        if (!string.IsNullOrWhiteSpace(request.StatusFilter) &&
            Enum.TryParse<ProjectStatus>(request.StatusFilter, out ProjectStatus status))
            query = query.Where(p => p.Status == status);

        query = query.OrderByDescending(p => p.CreatedAt);

        List<Project> projects = await query.ToListAsync(ct);

        return projects.Select(p => new ProjectDto(
            p.Id, p.CompanyId, p.Company.Name, p.Territory.Name,
            p.Sector, p.NumberOfTrees, p.ValidatedTrees,
            p.ProgressPercentage, p.Status.ToString(), p.CreatedAt
        )).ToList().AsReadOnly();
    }
}
