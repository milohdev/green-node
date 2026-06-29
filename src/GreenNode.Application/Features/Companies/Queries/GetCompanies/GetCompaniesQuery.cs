using GreenNode.Application.Features.Companies.DTOs;
using MediatR;

namespace GreenNode.Application.Features.Companies.Queries.GetCompanies;

public record GetCompaniesQuery() : IRequest<IReadOnlyList<CompanyDto>>;
