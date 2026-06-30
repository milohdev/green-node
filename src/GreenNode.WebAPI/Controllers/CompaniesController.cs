using GreenNode.Application.Features.Companies.DTOs;
using GreenNode.Application.Features.Companies.Queries.GetCompanies;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenNode.WebAPI.Controllers;

[ApiController]
[Route("api/companies")]
[Authorize(Roles = "Admin")]
public class CompaniesController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        IReadOnlyList<CompanyDto> result = await sender.Send(new GetCompaniesQuery(), ct);
        return Ok(new { data = result, message = "Empresas obtenidas." });
    }

    // GetCompanyByIdQuery es stub en Application — pendiente de implementación por Cami
    [HttpGet("{id:guid}")]
    public IActionResult GetById(Guid id)
    {
        return StatusCode(StatusCodes.Status501NotImplemented,
            new { message = "Pendiente: GetCompanyByIdQuery no está implementado aún en Application." });
    }

    // UpdateCompanyCommand es stub en Application — pendiente de implementación por Cami
    [HttpPut("{id:guid}")]
    public IActionResult Update(Guid id)
    {
        return StatusCode(StatusCodes.Status501NotImplemented,
            new { message = "Pendiente: UpdateCompanyCommand no está implementado aún en Application." });
    }
}
