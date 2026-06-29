namespace GreenNode.Application.Features.Companies.DTOs;

public record CompanyDto(
    Guid Id,
    Guid UserId,
    string Name,
    string Email,
    string Phone,
    string Sector,
    DateTime CreatedAt
);
