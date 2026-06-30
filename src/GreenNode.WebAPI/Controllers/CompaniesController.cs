using System.Security.Claims;
using GreenNode.Application.Common.Exceptions;
using GreenNode.Application.Features.Companies.Commands.UpdateCompany;
using GreenNode.Application.Features.Companies.DTOs;
using GreenNode.Application.Features.Companies.Queries.GetCompanies;
using GreenNode.Application.Features.Companies.Queries.GetCompanyById;
using GreenNode.WebAPI.Contracts.Companies;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenNode.WebAPI.Controllers;

[ApiController]
[Route("api/companies")]
[Authorize]
public class CompaniesController(ISender sender) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        IReadOnlyList<CompanyDto> result = await sender.Send(new GetCompaniesQuery(), ct);
        return Ok(new { data = result, message = "Empresas obtenidas." });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        Guid? callerCompanyId = GetCallerCompanyId();
        if (callerCompanyId.HasValue && callerCompanyId.Value != id)
            throw new ForbiddenException("No tienes acceso a esta empresa.");

        CompanyDto result = await sender.Send(new GetCompanyByIdQuery(id, callerCompanyId), ct);
        return Ok(new { data = result, message = "Empresa obtenida." });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateCompanyRequest request, CancellationToken ct)
    {
        Guid? callerCompanyId = GetCallerCompanyId();
        if (callerCompanyId.HasValue && callerCompanyId.Value != id)
            throw new ForbiddenException("No tienes acceso a esta empresa.");

        CompanyDto result = await sender.Send(
            new UpdateCompanyCommand(id, request.CompanyName, request.Phone, request.Sector, callerCompanyId), ct);
        return Ok(new { data = result, message = "Empresa actualizada." });
    }

    private Guid? GetCallerCompanyId()
    {
        if (User.IsInRole("Admin")) return null;
        string? claim = User.FindFirstValue("companyId");
        return claim is null ? null : Guid.Parse(claim);
    }
}
