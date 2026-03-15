using MediatR;

namespace Maba.Application.Features.Auth.Commands;

public class ResendVerificationCommand : IRequest<ResendVerificationResult>
{
    public string Email { get; set; } = string.Empty;
}

public class ResendVerificationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
