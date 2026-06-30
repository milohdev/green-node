using GreenNode.Application.Common.Exceptions;
using GreenNode.Application.Common.Interfaces;
using GreenNode.Application.Features.Companies.DTOs;
using GreenNode.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GreenNode.Application.Features.Companies.Queries.GetCompanyById;

// Implementado por Juan como fallback temporal — Cami debe revisar antes de merge a develop
public class GetCompanyByIdQueryHandler : IRequestHandler<GetCompanyByIdQuery, CompanyDto>
{
    private readonly IApplicationDbContext _context;

    public GetCompanyByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CompanyDto> Handle(GetCompanyByIdQuery request, CancellationToken ct)
    {
        Company company = await _context.Companies
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.CompanyId, ct)
            ?? throw new NotFoundException(nameof(Company), request.CompanyId);

        if (request.CallerCompanyId.HasValue && company.Id != request.CallerCompanyId.Value)
            throw new ForbiddenException("No tienes acceso a esta empresa.");

        return new CompanyDto(
            company.Id, company.UserId, company.Name,
            company.Email, company.Phone, company.Sector, company.CreatedAt);
    }
}
