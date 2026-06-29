namespace GreenNode.Application.Features.Quotes.DTOs;

public record QuoteDto(
    Guid Id,
    Guid CompanyId,
    string CompanyName,
    string TerritoryName,
    string Sector,
    int NumberOfTrees,
    decimal TotalPrice,
    string Status,
    DateTime CreatedAt
);
