using GreenNode.Application.Features.Advisor.Commands.AskAdvisor.DTOs;
using MediatR;

namespace GreenNode.Application.Features.Advisor.Commands.AskAdvisor;

public record AskAdvisorCommand(
    Guid CompanyId,
    string Question
) : IRequest<AdvisorResponseDto>;
