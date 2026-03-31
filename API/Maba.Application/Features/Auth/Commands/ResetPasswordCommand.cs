using MediatR;

namespace Maba.Application.Features.Auth.Commands;

public class ResetPasswordCommand : IRequest<Unit>
{
    public string? Email { get; set; }
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

