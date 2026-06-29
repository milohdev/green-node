namespace GreenNode.Application.Common.Models;

public record CertificateData(
    string CompanyName,
    string TerritoryName,
    int NumberOfTrees,
    decimal Co2EstimatedKg,
    string VerificationCode,
    DateTime IssuedAt
);
