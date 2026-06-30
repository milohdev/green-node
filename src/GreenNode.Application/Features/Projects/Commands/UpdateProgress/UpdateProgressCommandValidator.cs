using FluentValidation;

namespace GreenNode.Application.Features.Projects.Commands.UpdateProgress;

// Implementado por Juan como fallback temporal — Cami debe revisar antes de merge a develop
public class UpdateProgressCommandValidator : AbstractValidator<UpdateProgressCommand>
{
    public UpdateProgressCommandValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.ValidatedTreeCount)
            .GreaterThan(0)
            .WithMessage("El número de árboles validados debe ser mayor a cero.");
    }
}
