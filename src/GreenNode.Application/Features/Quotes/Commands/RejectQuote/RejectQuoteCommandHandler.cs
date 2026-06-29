using GreenNode.Application.Common.Exceptions;
using GreenNode.Application.Common.Interfaces;
using GreenNode.Application.Features.Quotes.DTOs;
using GreenNode.Domain.Entities;
using GreenNode.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GreenNode.Application.Features.Quotes.Commands.RejectQuote;

public class RejectQuoteCommandHandler : IRequestHandler<RejectQuoteCommand, QuoteDetailDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<RejectQuoteCommandHandler> _logger;

    public RejectQuoteCommandHandler(
        IApplicationDbContext context,
        IEmailService emailService,
        ILogger<RejectQuoteCommandHandler> logger)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<QuoteDetailDto> Handle(RejectQuoteCommand request, CancellationToken ct)
    {
        Quote quote = await _context.Quotes
            .Include(q => q.Company)
            .Include(q => q.Territory)
            .FirstOrDefaultAsync(q => q.Id == request.QuoteId, ct)
            ?? throw new NotFoundException(nameof(Quote), request.QuoteId);

        if (quote.Status != QuoteStatus.Pending)
            throw new AppValidationException(
                $"Solo se puede rechazar una cotización en estado Pending. Estado actual: {quote.Status}");

        quote.Reject(request.Reason);

        await _context.SaveChangesAsync(ct);

        _ = _emailService.SendQuoteRejectedEmailAsync(
            quote.Company.Email, quote.Company.Name,
            request.Reason, CancellationToken.None);

        _logger.LogInformation("Quote {QuoteId} rechazada", quote.Id);

        return new QuoteDetailDto(
            quote.Id, quote.CompanyId, quote.Company.Name,
            quote.TerritoryId, quote.Territory.Name,
            quote.Territory.Department, quote.Territory.Municipality,
            quote.Sector, quote.NumberOfTrees, quote.TotalPrice,
            quote.Status.ToString(), quote.AiProposal,
            quote.RejectionReason,
            ProjectId: null,
            quote.CreatedAt);
    }
}
