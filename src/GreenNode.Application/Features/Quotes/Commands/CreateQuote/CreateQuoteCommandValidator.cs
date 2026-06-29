using FluentValidation;

namespace GreenNode.Application.Features.Quotes.Commands.CreateQuote;

public class CreateQuoteCommandValidator : AbstractValidator<CreateQuoteCommand>
{
    public CreateQuoteCommandValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.TerritoryId)
            .NotEmpty()
            .WithMessage("El territorio es obligatorio.");
        RuleFor(x => x.Sector)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("El sector es obligatorio.");
        RuleFor(x => x.NumberOfTrees)
            .GreaterThan(0)
            .WithMessage("El número de árboles debe ser mayor a cero.");
    }
}
