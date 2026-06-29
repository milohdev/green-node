namespace GreenNode.Application.Features.Auth.DTOs;

public record CompanyProfileDto(
    Guid UserId,
    Guid CompanyId,
    string CompanyName,
    string Email,
    string Sector
);
