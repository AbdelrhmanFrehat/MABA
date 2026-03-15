using MediatR;

namespace Maba.Application.Features.Roles.Commands;

public class DeleteRoleCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
}

