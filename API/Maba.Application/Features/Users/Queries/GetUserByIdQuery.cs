using MediatR;
using Maba.Application.Features.Users.DTOs;

namespace Maba.Application.Features.Users.Queries;

public class GetUserByIdQuery : IRequest<UserDto>
{
    public Guid Id { get; set; }
}

