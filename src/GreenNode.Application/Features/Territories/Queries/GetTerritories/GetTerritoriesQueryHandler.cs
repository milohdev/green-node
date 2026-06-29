using GreenNode.Application.Common.Interfaces;
using GreenNode.Application.Features.Territories.Queries.GetTerritories.DTOs;
using GreenNode.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GreenNode.Application.Features.Territories.Queries.GetTerritories;

public class GetTerritoriesQueryHandler
    : IRequestHandler<GetTerritoriesQuery, IReadOnlyList<TerritoryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetTerritoriesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<TerritoryDto>> Handle(
        GetTerritoriesQuery request, CancellationToken ct)
    {
        List<Territory> territories = await _context.Territories
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .ToListAsync(ct);

        return territories.Select(t => new TerritoryDto(
            t.Id, t.Name, t.Department, t.Municipality,
            t.AvailableHectares, t.PricePerTree
        )).ToList().AsReadOnly();
    }
}
