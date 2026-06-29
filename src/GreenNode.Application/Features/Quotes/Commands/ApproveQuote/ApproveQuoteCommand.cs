using GreenNode.Application.Features.Quotes.DTOs;
using MediatR;

namespace GreenNode.Application.Features.Quotes.Commands.ApproveQuote;

public record ApproveQuoteCommand(Guid QuoteId) : IRequest<QuoteDetailDto>;
