using FluentValidation;

namespace GreenNode.Application.Features.Evidences.Commands.UploadEvidence;

public class UploadEvidenceCommandValidator : AbstractValidator<UploadEvidenceCommand>
{
    public UploadEvidenceCommandValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.ContentType)
            .NotEmpty()
            .Must(ct => ct.StartsWith("image/"))
            .WithMessage("Solo se permiten archivos de imagen.");
    }
}
