using GreenNode.Application.Common.Interfaces;
using GreenNode.Application.Features.Companies.DTOs;
using GreenNode.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GreenNode.Application.Features.Companies.Queries.GetCompanies;

public class GetCompaniesQueryHandler : IRequestHandler<GetCompaniesQuery, IReadOnlyList<CompanyDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCompaniesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CompanyDto>> Handle(GetCompaniesQuery request, CancellationToken ct)
    {
        List<Company> companies = await _context.Companies
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync(ct);

        return companies.Select(c => new CompanyDto(
            c.Id, c.UserId, c.Name, c.Email, c.Phone, c.Sector, c.CreatedAt
        )).ToList().AsReadOnly();
    }
}
