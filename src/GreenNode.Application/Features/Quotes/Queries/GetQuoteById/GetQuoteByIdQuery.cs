using GreenNode.Application.Features.Quotes.DTOs;
using MediatR;

namespace GreenNode.Application.Features.Quotes.Queries.GetQuoteById;

public record GetQuoteByIdQuery(
    Guid QuoteId,
    Guid? CompanyId
) : IRequest<QuoteDetailDto>;
