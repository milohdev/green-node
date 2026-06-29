using GreenNode.Application.Common.Exceptions;
using GreenNode.Application.Common.Interfaces;
using GreenNode.Application.Features.Quotes.DTOs;
using GreenNode.Domain.Entities;
using GreenNode.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GreenNode.Application.Features.Quotes.Commands.ApproveQuote;

public class ApproveQuoteCommandHandler : IRequestHandler<ApproveQuoteCommand, QuoteDetailDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<ApproveQuoteCommandHandler> _logger;

    public ApproveQuoteCommandHandler(
        IApplicationDbContext context,
        IEmailService emailService,
        ILogger<ApproveQuoteCommandHandler> logger)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<QuoteDetailDto> Handle(ApproveQuoteCommand request, CancellationToken ct)
    {
        Quote quote = await _context.Quotes
            .Include(q => q.Company)
            .Include(q => q.Territory)
            .FirstOrDefaultAsync(q => q.Id == request.QuoteId, ct)
            ?? throw new NotFoundException(nameof(Quote), request.QuoteId);

        if (quote.Status != QuoteStatus.Pending)
            throw new AppValidationException(
                $"Solo se puede aprobar una cotización en estado Pending. Estado actual: {quote.Status}");

        quote.Approve();

        Project project = new(quote.Id, quote.CompanyId, quote.TerritoryId,
            quote.Sector, quote.NumberOfTrees);
        _context.Projects.Add(project);

        await _context.SaveChangesAsync(ct);

        _ = _emailService.SendQuoteApprovedEmailAsync(
            quote.Company.Email, quote.Company.Name,
            project.Id, CancellationToken.None);

        _logger.LogInformation("Quote {QuoteId} aprobada — Project {ProjectId} creado", quote.Id, project.Id);

        return new QuoteDetailDto(
            quote.Id, quote.CompanyId, quote.Company.Name,
            quote.TerritoryId, quote.Territory.Name,
            quote.Territory.Department, quote.Territory.Municipality,
            quote.Sector, quote.NumberOfTrees, quote.TotalPrice,
            quote.Status.ToString(), quote.AiProposal,
            quote.RejectionReason,
            ProjectId: project.Id,
            quote.CreatedAt);
    }
}
