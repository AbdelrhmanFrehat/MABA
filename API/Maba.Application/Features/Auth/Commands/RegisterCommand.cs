using MediatR;
using Maba.Application.Features.Auth.DTOs;

namespace Maba.Application.Features.Auth.Commands;

public class RegisterCommand : IRequest<RegisterResponseDto>
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Password { get; set; } = string.Empty;
}

