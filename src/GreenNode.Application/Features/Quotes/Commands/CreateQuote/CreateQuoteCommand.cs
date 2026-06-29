using GreenNode.Application.Features.Quotes.DTOs;
using MediatR;

namespace GreenNode.Application.Features.Quotes.Commands.CreateQuote;

public record CreateQuoteCommand(
    Guid CompanyId,
    Guid TerritoryId,
    string Sector,
    int NumberOfTrees
) : IRequest<QuoteDetailDto>;
