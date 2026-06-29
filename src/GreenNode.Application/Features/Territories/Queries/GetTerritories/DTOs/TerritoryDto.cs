namespace GreenNode.Application.Features.Territories.Queries.GetTerritories.DTOs;

public record TerritoryDto(
    Guid Id,
    string Name,
    string Department,
    string Municipality,
    decimal AvailableHectares,
    decimal PricePerTree
);
