using FluentValidation;

namespace GreenNode.Application.Features.Quotes.Commands.ApproveQuote;

public class ApproveQuoteCommandValidator : AbstractValidator<ApproveQuoteCommand>
{
    public ApproveQuoteCommandValidator()
    {
        RuleFor(x => x.QuoteId).NotEmpty();
    }
}
