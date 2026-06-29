using GreenNode.Application.Features.Quotes.DTOs;
using MediatR;

namespace GreenNode.Application.Features.Quotes.Commands.RejectQuote;

public record RejectQuoteCommand(
    Guid QuoteId,
    string Reason
) : IRequest<QuoteDetailDto>;
