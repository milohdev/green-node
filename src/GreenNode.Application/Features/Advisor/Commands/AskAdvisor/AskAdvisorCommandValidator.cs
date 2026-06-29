using FluentValidation;

namespace GreenNode.Application.Features.Advisor.Commands.AskAdvisor;

public class AskAdvisorCommandValidator : AbstractValidator<AskAdvisorCommand>
{
    public AskAdvisorCommandValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.Question)
            .NotEmpty()
            .MaximumLength(1000)
            .WithMessage("La pregunta no puede estar vacía.");
    }
}
