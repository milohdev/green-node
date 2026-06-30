using System.Security.Claims;
using GreenNode.Application.Common.Exceptions;
using GreenNode.Application.Features.Companies.DTOs;
using GreenNode.Application.Features.Companies.Queries.GetCompanies;
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

    // TODO: pendiente GetCompanyByIdQuery de Cami (actualmente clase vacía sin IRequest<T>)
    [HttpGet("{id:guid}")]
    public IActionResult GetById(Guid id)
    {
        if (!User.IsInRole("Admin") && GetCompanyId() != id)
            throw new ForbiddenException("No tienes acceso a esta empresa.");

        return StatusCode(StatusCodes.Status501NotImplemented,
            new { message = "Pendiente: GetCompanyByIdQuery no está implementado aún en Application." });
    }

    // TODO: pendiente UpdateCompanyCommand de Cami (actualmente clase vacía sin IRequest<T>)
    [HttpPut("{id:guid}")]
    public IActionResult Update(Guid id)
    {
        if (!User.IsInRole("Admin") && GetCompanyId() != id)
            throw new ForbiddenException("No tienes acceso a esta empresa.");

        return StatusCode(StatusCodes.Status501NotImplemented,
            new { message = "Pendiente: UpdateCompanyCommand no está implementado aún en Application." });
    }

    private Guid GetCompanyId() =>
        Guid.Parse(User.FindFirstValue("companyId")!);
}
