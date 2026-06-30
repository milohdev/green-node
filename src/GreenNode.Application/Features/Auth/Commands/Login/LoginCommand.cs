using GreenNode.Application.Features.Auth.DTOs;
using MediatR;

namespace GreenNode.Application.Features.Auth.Commands.Login;

public record LoginCommand(
    string Email,
    string Password
) : IRequest<AuthDto>;
