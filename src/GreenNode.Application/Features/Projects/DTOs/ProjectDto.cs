namespace GreenNode.Application.Features.Projects.DTOs;

public record ProjectDto(
    Guid Id,
    Guid CompanyId,
    string CompanyName,
    string TerritoryName,
    string Sector,
    int NumberOfTrees,
    int ValidatedTrees,
    decimal ProgressPercentage,
    string Status,
    DateTime CreatedAt
);
