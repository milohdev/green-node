using GreenNode.Application.Features.Companies.DTOs;
using MediatR;

namespace GreenNode.Application.Features.Companies.Commands.UpdateCompany;

// Implementado por Juan como fallback temporal — Cami debe revisar antes de merge a develop
public record UpdateCompanyCommand(
    Guid CompanyId,
    string CompanyName,
    string Phone,
    string Sector,
    Guid? CallerCompanyId
) : IRequest<CompanyDto>;
