using GreenNode.Application.Common.Exceptions;
using GreenNode.Application.Common.Interfaces;
using GreenNode.Application.Features.Advisor.Commands.AskAdvisor.DTOs;
using GreenNode.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GreenNode.Application.Features.Advisor.Commands.AskAdvisor;

public class AskAdvisorCommandHandler : IRequestHandler<AskAdvisorCommand, AdvisorResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IClaudeAiService _aiService;
    private readonly ILogger<AskAdvisorCommandHandler> _logger;

    public AskAdvisorCommandHandler(
        IApplicationDbContext context,
        IClaudeAiService aiService,
        ILogger<AskAdvisorCommandHandler> logger)
    {
        _context = context;
        _aiService = aiService;
        _logger = logger;
    }

    public async Task<AdvisorResponseDto> Handle(AskAdvisorCommand request, CancellationToken ct)
    {
        Project? project = await _context.Projects
            .AsNoTracking()
            .Include(p => p.Company)
            .Include(p => p.Territory)
            .Where(p => p.CompanyId == request.CompanyId)
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (project is null)
            throw new NotFoundException("Proyecto", $"empresa {request.CompanyId}");

        string response = await _aiService.GetProjectAdvisorResponseAsync(
            companyName:    project.Company.Name,
            sector:         project.Sector,
            numberOfTrees:  project.NumberOfTrees,
            validatedTrees: project.ValidatedTrees,
            progress:       project.ProgressPercentage,
            userQuestion:   request.Question,
            ct:             ct);

        _logger.LogInformation("Asesor IA respondió para empresa {CompanyId}", request.CompanyId);

        return new AdvisorResponseDto(
            response,
            project.Company.Name,
            project.Sector,
            project.ProgressPercentage);
    }
}
