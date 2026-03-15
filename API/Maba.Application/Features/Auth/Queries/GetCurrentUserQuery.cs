using MediatR;
using Maba.Application.Features.Auth.DTOs;

namespace Maba.Application.Features.Auth.Queries;

public class GetCurrentUserQuery : IRequest<UserDto>
{
    public Guid UserId { get; set; }
}

