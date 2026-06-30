using GreenNode.Application.Features.Projects.DTOs;
using MediatR;

namespace GreenNode.Application.Features.Projects.Commands.UpdateProgress;

// Implementado por Juan como fallback temporal — Cami debe revisar antes de merge a develop
public record UpdateProgressCommand(
    Guid ProjectId,
    int ValidatedTreeCount
) : IRequest<ProjectDetailDto>;
