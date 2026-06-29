using GreenNode.Application.Features.Projects.DTOs;
using MediatR;

namespace GreenNode.Application.Features.Projects.Queries.GetProjects;

public record GetProjectsQuery(
    Guid? CompanyId,
    string? StatusFilter
) : IRequest<IReadOnlyList<ProjectDto>>;
