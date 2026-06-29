using FluentValidation;

namespace GreenNode.Application.Features.Certificates.Commands.GenerateCertificate;

public class GenerateCertificateCommandValidator : AbstractValidator<GenerateCertificateCommand>
{
    public GenerateCertificateCommandValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
    }
}
