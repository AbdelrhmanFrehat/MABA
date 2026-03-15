using MediatR;

namespace Maba.Application.Features.Users.Commands;

public class DeleteUserCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
}

