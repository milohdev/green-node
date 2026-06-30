using FluentValidation;

namespace GreenNode.Application.Features.Evidences.Commands.UploadEvidence;

public class UploadEvidenceCommandValidator : AbstractValidator<UploadEvidenceCommand>
{
    public UploadEvidenceCommandValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        // CallerCompanyId es nullable: null = Admin, con valor = Company. No se valida NotEmpty.
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.ContentType)
            .NotEmpty()
            .Must(ct => ct.StartsWith("image/"))
            .WithMessage("Solo se permiten archivos de imagen.");
    }
}
