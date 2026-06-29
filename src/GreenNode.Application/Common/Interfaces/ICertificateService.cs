using GreenNode.Application.Common.Models;

namespace GreenNode.Application.Common.Interfaces;

public interface ICertificateService
{
    Task<string> GenerateCertificateAsync(
        CertificateData data,
        CancellationToken ct = default);
}
