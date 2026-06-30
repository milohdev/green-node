namespace GreenNode.Application.Features.Certificates.DTOs;

// Implementado por Juan como fallback temporal — Cami debe revisar antes de merge a develop
public record CertificateDto(
    Guid Id,
    Guid ProjectId,
    string CompanyName,
    string TerritoryName,
    int NumberOfTrees,
    decimal Co2EstimatedKg,
    string PdfUrl,
    string VerificationCode,
    DateTime IssuedAt,
    DateTime CreatedAt
);
