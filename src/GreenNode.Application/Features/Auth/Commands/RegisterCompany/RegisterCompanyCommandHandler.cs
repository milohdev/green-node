using GreenNode.Application.Common.Exceptions;
using GreenNode.Application.Common.Interfaces;
using GreenNode.Application.Features.Auth.DTOs;
using GreenNode.Domain.Entities;
using GreenNode.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GreenNode.Application.Features.Auth.Commands.RegisterCompany;

public class RegisterCompanyCommandHandler : IRequestHandler<RegisterCompanyCommand, CompanyProfileDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<RegisterCompanyCommandHandler> _logger;

    public RegisterCompanyCommandHandler(
        IApplicationDbContext context,
        IEmailService emailService,
        ILogger<RegisterCompanyCommandHandler> logger)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<CompanyProfileDto> Handle(
        RegisterCompanyCommand request, CancellationToken ct)
    {
        bool emailExists = await _context.Users
            .AnyAsync(u => u.Email == request.Email, ct);

        if (emailExists)
            throw new AppValidationException("El email ya está registrado.");

        string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        User user = new(request.CompanyName, request.Email, passwordHash, UserRole.Company);
        Company company = new(request.CompanyName, request.Email, request.Phone, request.Sector, user.Id);

        _context.Users.Add(user);
        _context.Companies.Add(company);
        await _context.SaveChangesAsync(ct);

        _ = _emailService.SendQuoteReceivedEmailAsync(
            user.Email, company.Name, Guid.Empty, CancellationToken.None);

        _logger.LogInformation("Empresa registrada: {CompanyId}", company.Id);

        return new CompanyProfileDto(user.Id, company.Id, company.Name, user.Email, company.Sector);
    }
}
