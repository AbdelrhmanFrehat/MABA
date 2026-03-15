using MediatR;

namespace Maba.Application.Features.Auth.Commands;

public class UpdateEmailCommand : IRequest<Unit>
{
    public Guid UserId { get; set; }
    public string NewEmail { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

