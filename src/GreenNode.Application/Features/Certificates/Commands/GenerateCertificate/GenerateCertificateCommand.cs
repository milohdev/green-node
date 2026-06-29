using MediatR;

namespace GreenNode.Application.Features.Certificates.Commands.GenerateCertificate;

public record GenerateCertificateCommand(Guid ProjectId) : IRequest<string>;
