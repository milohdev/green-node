using FluentValidation;

namespace GreenNode.Application.Features.Quotes.Commands.RejectQuote;

public class RejectQuoteCommandValidator : AbstractValidator<RejectQuoteCommand>
{
    public RejectQuoteCommandValidator()
    {
        RuleFor(x => x.QuoteId).NotEmpty();
        RuleFor(x => x.Reason)
            .NotEmpty()
            .MaximumLength(1000)
            .WithMessage("El motivo de rechazo es obligatorio.");
    }
}
