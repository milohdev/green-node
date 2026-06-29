using GreenNode.Application.Features.Evidences.DTOs;

namespace GreenNode.Application.Features.Projects.DTOs;

public record ProjectDetailDto(
    Guid Id,
    Guid CompanyId,
    string CompanyName,
    Guid QuoteId,
    string TerritoryName,
    string Department,
    string Municipality,
    string Sector,
    int NumberOfTrees,
    int ValidatedTrees,
    decimal ProgressPercentage,
    string Status,
    string? CertificateUrl,
    IReadOnlyList<EvidenceDto> Evidences,
    DateTime CreatedAt
);
