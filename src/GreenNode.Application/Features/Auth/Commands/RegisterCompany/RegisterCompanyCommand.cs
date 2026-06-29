using GreenNode.Application.Features.Auth.DTOs;
using MediatR;

namespace GreenNode.Application.Features.Auth.Commands.RegisterCompany;

public record RegisterCompanyCommand(
    string CompanyName,
    string Email,
    string Password,
    string Phone,
    string Sector
) : IRequest<CompanyProfileDto>;
