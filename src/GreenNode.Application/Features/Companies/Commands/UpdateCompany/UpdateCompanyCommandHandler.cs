using GreenNode.Application.Common.Exceptions;
using GreenNode.Application.Common.Interfaces;
using GreenNode.Application.Features.Companies.DTOs;
using GreenNode.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GreenNode.Application.Features.Companies.Commands.UpdateCompany;

// Implementado por Juan como fallback temporal — Cami debe revisar antes de merge a develop
public class UpdateCompanyCommandHandler : IRequestHandler<UpdateCompanyCommand, CompanyDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateCompanyCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CompanyDto> Handle(UpdateCompanyCommand request, CancellationToken ct)
    {
        Company company = await _context.Companies
            .FirstOrDefaultAsync(c => c.Id == request.CompanyId, ct)
            ?? throw new NotFoundException(nameof(Company), request.CompanyId);

        if (request.CallerCompanyId.HasValue && company.Id != request.CallerCompanyId.Value)
            throw new ForbiddenException("No tienes acceso a esta empresa.");

        company.Update(request.CompanyName, request.Phone, request.Sector);
        await _context.SaveChangesAsync(ct);

        return new CompanyDto(
            company.Id, company.UserId, company.Name,
            company.Email, company.Phone, company.Sector, company.CreatedAt);
    }
}
