namespace GreenNode.WebAPI.Contracts.Auth;

public record LoginRequest(string Email, string Password);

public record RegisterCompanyRequest(
    string CompanyName,
    string Email,
    string Password,
    string Phone,
    string Sector);
