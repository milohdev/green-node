using GreenNode.Application.Features.Auth.Commands.Login;
using GreenNode.Application.Features.Auth.Commands.RegisterCompany;
using GreenNode.Application.Features.Auth.DTOs;
using GreenNode.WebAPI.Contracts.Auth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenNode.WebAPI.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(ISender sender) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterCompanyRequest request, CancellationToken ct)
    {
        CompanyProfileDto result = await sender.Send(
            new RegisterCompanyCommand(
                request.CompanyName,
                request.Email,
                request.Password,
                request.Phone,
                request.Sector), ct);

        return Ok(new { data = result, message = "Empresa registrada exitosamente." });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken ct)
    {
        AuthDto result = await sender.Send(
            new LoginCommand(request.Email, request.Password), ct);

        return Ok(new { data = result, message = "Inicio de sesión exitoso." });
    }
}
