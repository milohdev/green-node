using GreenNode.Application.Features.Companies.DTOs;
using MediatR;

namespace GreenNode.Application.Features.Companies.Queries.GetCompanyById;

public record GetCompanyByIdQuery(
    Guid CompanyId,
    Guid? CallerCompanyId
) : IRequest<CompanyDto>;
