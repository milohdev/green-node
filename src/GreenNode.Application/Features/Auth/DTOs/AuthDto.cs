namespace GreenNode.Application.Features.Auth.DTOs;

public record AuthDto(
    string Token,
    string Email,
    string Role,
    Guid? CompanyId
);
