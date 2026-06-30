using System.Security.Claims;
using GreenNode.Application.Features.Quotes.Commands.ApproveQuote;
using GreenNode.Application.Features.Quotes.Commands.CreateQuote;
using GreenNode.Application.Features.Quotes.Commands.RejectQuote;
using GreenNode.Application.Features.Quotes.DTOs;
using GreenNode.Application.Features.Quotes.Queries.GetQuoteById;
using GreenNode.Application.Features.Quotes.Queries.GetQuotes;
using GreenNode.WebAPI.Contracts.Quotes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenNode.WebAPI.Controllers;

[ApiController]
[Route("api/quotes")]
[Authorize]
public class QuotesController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? statusFilter, CancellationToken ct)
    {
        Guid? companyId = User.IsInRole("Admin") ? null : GetCompanyId();
        IReadOnlyList<QuoteDto> result = await sender.Send(
            new GetQuotesQuery(companyId, statusFilter), ct);
        return Ok(new { data = result, message = "Cotizaciones obtenidas." });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        Guid? companyId = User.IsInRole("Admin") ? null : GetCompanyId();
        QuoteDetailDto result = await sender.Send(new GetQuoteByIdQuery(id, companyId), ct);
        return Ok(new { data = result, message = "Cotización obtenida." });
    }

    [HttpPost]
    [Authorize(Roles = "Company")]
    public async Task<IActionResult> Create(CreateQuoteRequest request, CancellationToken ct)
    {
        Guid companyId = GetCompanyId();
        QuoteDetailDto result = await sender.Send(
            new CreateQuoteCommand(companyId, request.TerritoryId, request.Sector, request.NumberOfTrees), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            new { data = result, message = "Cotización creada." });
    }

    [HttpPost("{id:guid}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Approve(Guid id, CancellationToken ct)
    {
        QuoteDetailDto result = await sender.Send(new ApproveQuoteCommand(id), ct);
        return Ok(new { data = result, message = "Cotización aprobada." });
    }

    [HttpPost("{id:guid}/reject")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Reject(Guid id, RejectQuoteRequest request, CancellationToken ct)
    {
        QuoteDetailDto result = await sender.Send(new RejectQuoteCommand(id, request.Reason), ct);
        return Ok(new { data = result, message = "Cotización rechazada." });
    }

    private Guid GetCompanyId() =>
        Guid.Parse(User.FindFirstValue("companyId")!);
}
