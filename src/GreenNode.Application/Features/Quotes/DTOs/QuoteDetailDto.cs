namespace GreenNode.Application.Features.Quotes.DTOs;

public record QuoteDetailDto(
    Guid Id,
    Guid CompanyId,
    string CompanyName,
    Guid TerritoryId,
    string TerritoryName,
    string Department,
    string Municipality,
    string Sector,
    int NumberOfTrees,
    decimal TotalPrice,
    string Status,
    string? AiProposal,
    string? RejectionReason,
    Guid? ProjectId,
    DateTime CreatedAt
);
