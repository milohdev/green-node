using FluentValidation;

namespace GreenNode.Application.Features.Companies.Commands.UpdateCompany;

// Implementado por Juan como fallback temporal — Cami debe revisar antes de merge a develop
public class UpdateCompanyCommandValidator : AbstractValidator<UpdateCompanyCommand>
{
    public UpdateCompanyCommandValidator()
    {
        RuleFor(x => x.CompanyName)
            .NotEmpty()
            .MaximumLength(200)
            .WithMessage("El nombre de la empresa es obligatorio.");
        RuleFor(x => x.Phone)
            .NotEmpty()
            .MaximumLength(20)
            .WithMessage("El teléfono es obligatorio.");
        RuleFor(x => x.Sector)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("El sector es obligatorio.");
    }
}
