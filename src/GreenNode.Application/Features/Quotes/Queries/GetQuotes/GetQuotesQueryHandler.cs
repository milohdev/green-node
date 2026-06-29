using GreenNode.Application.Common.Interfaces;
using GreenNode.Application.Features.Quotes.DTOs;
using GreenNode.Domain.Entities;
using GreenNode.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GreenNode.Application.Features.Quotes.Queries.GetQuotes;

public class GetQuotesQueryHandler : IRequestHandler<GetQuotesQuery, IReadOnlyList<QuoteDto>>
{
    private readonly IApplicationDbContext _context;

    public GetQuotesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<QuoteDto>> Handle(GetQuotesQuery request, CancellationToken ct)
    {
        IQueryable<Quote> query = _context.Quotes
            .AsNoTracking()
            .Include(q => q.Company)
            .Include(q => q.Territory);

        if (request.CompanyId.HasValue)
            query = query.Where(q => q.CompanyId == request.CompanyId.Value);

        if (!string.IsNullOrWhiteSpace(request.StatusFilter) &&
            Enum.TryParse<QuoteStatus>(request.StatusFilter, out QuoteStatus status))
            query = query.Where(q => q.Status == status);

        query = query.OrderByDescending(q => q.CreatedAt);

        List<Quote> quotes = await query.ToListAsync(ct);

        return quotes.Select(q => new QuoteDto(
            q.Id, q.CompanyId, q.Company.Name, q.Territory.Name,
            q.Sector, q.NumberOfTrees, q.TotalPrice,
            q.Status.ToString(), q.CreatedAt
        )).ToList().AsReadOnly();
    }
}
