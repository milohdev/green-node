using GreenNode.Application.Common.Exceptions;
using GreenNode.Application.Common.Interfaces;
using GreenNode.Application.Features.Quotes.DTOs;
using GreenNode.Domain.Entities;
using GreenNode.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GreenNode.Application.Features.Quotes.Queries.GetQuoteById;

public class GetQuoteByIdQueryHandler : IRequestHandler<GetQuoteByIdQuery, QuoteDetailDto>
{
    private readonly IApplicationDbContext _context;

    public GetQuoteByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<QuoteDetailDto> Handle(GetQuoteByIdQuery request, CancellationToken ct)
    {
        Quote quote = await _context.Quotes
            .AsNoTracking()
            .Include(q => q.Company)
            .Include(q => q.Territory)
            .FirstOrDefaultAsync(q => q.Id == request.QuoteId, ct)
            ?? throw new NotFoundException(nameof(Quote), request.QuoteId);

        if (request.CompanyId.HasValue && quote.CompanyId != request.CompanyId.Value)
            throw new ForbiddenException("No tienes acceso a esta cotización.");

        Guid? projectId = null;
        if (quote.Status == QuoteStatus.Approved)
        {
            Project? project = await _context.Projects
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.QuoteId == quote.Id, ct);
            projectId = project?.Id;
        }

        return new QuoteDetailDto(
            quote.Id, quote.CompanyId, quote.Company.Name,
            quote.TerritoryId, quote.Territory.Name,
            quote.Territory.Department, quote.Territory.Municipality,
            quote.Sector, quote.NumberOfTrees, quote.TotalPrice,
            quote.Status.ToString(), quote.AiProposal,
            quote.RejectionReason, projectId,
            quote.CreatedAt);
    }
}
