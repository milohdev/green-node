using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GreenNode.Application.Common.Interfaces;
using GreenNode.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace GreenNode.Infrastructure.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly string _secret;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expirationMinutes;

    public JwtTokenService(IConfiguration configuration)
    {
        _secret = configuration["JwtSettings:Secret"]
            ?? throw new InvalidOperationException("JwtSettings:Secret no está configurado.");
        _issuer = configuration["JwtSettings:Issuer"]
            ?? throw new InvalidOperationException("JwtSettings:Issuer no está configurado.");
        _audience = configuration["JwtSettings:Audience"]
            ?? throw new InvalidOperationException("JwtSettings:Audience no está configurado.");
        _expirationMinutes = int.Parse(
            configuration["JwtSettings:ExpirationMinutes"] ?? "60");
    }

    public string GenerateToken(User user, Guid? companyId)
    {
        List<Claim> claims =
        [
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
        ];

        if (companyId.HasValue)
            claims.Add(new Claim("companyId", companyId.Value.ToString()));

        SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(_secret));
        SigningCredentials creds = new(key, SecurityAlgorithms.HmacSha256);

        JwtSecurityToken token = new(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_expirationMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
