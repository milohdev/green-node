using GreenNode.Application.Common.Exceptions;
using GreenNode.Application.Common.Interfaces;
using GreenNode.Application.Features.Auth.DTOs;
using GreenNode.Domain.Entities;
using GreenNode.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GreenNode.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IApplicationDbContext context,
        IJwtTokenService jwtTokenService,
        ILogger<LoginCommandHandler> logger)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    public async Task<AuthDto> Handle(LoginCommand request, CancellationToken ct)
    {
        User? user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == request.Email, ct);

        if (user is null)
            throw new AppValidationException("Credenciales inválidas.");

        bool valid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

        if (!valid)
            throw new AppValidationException("Credenciales inválidas.");

        Guid? companyId = null;
        if (user.Role == UserRole.Company)
        {
            Company? company = await _context.Companies
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.UserId == user.Id, ct);
            companyId = company?.Id;
        }

        string token = _jwtTokenService.GenerateToken(user, companyId);

        _logger.LogInformation("Login exitoso: {UserId}", user.Id);

        return new AuthDto(token, user.Email, user.Role.ToString(), companyId);
    }
}
