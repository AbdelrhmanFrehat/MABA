using MediatR;

namespace Maba.Application.Features.Media.Commands;

public class DeleteMediaCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
}

