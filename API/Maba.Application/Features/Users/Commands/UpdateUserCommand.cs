using MediatR;
using Maba.Application.Features.Users.DTOs;

namespace Maba.Application.Features.Users.Commands;

public class UpdateUserCommand : IRequest<UserDto>
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
}

