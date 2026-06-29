namespace GreenNode.Application.Common.Interfaces;

public record CertificateData(
    string CompanyName,
    string TerritoryName,
    int NumberOfTrees,
    decimal Co2EstimatedKg,
    string VerificationCode,
    DateTime IssuedAt
);

public interface ICertificateService
{
    Task<string> GenerateCertificateAsync(CertificateData data);
}
