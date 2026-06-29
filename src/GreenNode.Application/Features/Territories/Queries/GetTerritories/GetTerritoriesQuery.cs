using GreenNode.Application.Features.Territories.Queries.GetTerritories.DTOs;
using MediatR;

namespace GreenNode.Application.Features.Territories.Queries.GetTerritories;

public record GetTerritoriesQuery() : IRequest<IReadOnlyList<TerritoryDto>>;
