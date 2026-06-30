using GreenNode.Application.Features.Projects.DTOs;
using MediatR;

namespace GreenNode.Application.Features.Projects.Queries.GetProjectById;

public record GetProjectByIdQuery(
    Guid ProjectId,
    Guid? CompanyId
) : IRequest<ProjectDetailDto>;
