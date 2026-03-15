using MediatR;

namespace Maba.Application.Features.Auth.Commands;

public class VerifyEmailCommand : IRequest<VerifyEmailResult>
{
    public string Token { get; set; } = string.Empty;
}

public class VerifyEmailResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
