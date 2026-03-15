using MediatR;
using Maba.Application.Features.Users.DTOs;

namespace Maba.Application.Features.Users.Queries;

public class GetAllUsersQuery : IRequest<List<UserDto>>
{
    public bool? IsActive { get; set; }
}

