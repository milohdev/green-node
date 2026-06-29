using GreenNode.Application.Features.Quotes.DTOs;
using MediatR;

namespace GreenNode.Application.Features.Quotes.Queries.GetQuotes;

public record GetQuotesQuery(
    Guid? CompanyId,
    string? StatusFilter
) : IRequest<IReadOnlyList<QuoteDto>>;
