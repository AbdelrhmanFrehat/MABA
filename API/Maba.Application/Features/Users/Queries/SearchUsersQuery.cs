using MediatR;
using Maba.Application.Common.Models;
using Maba.Application.Features.Users.DTOs;

namespace Maba.Application.Features.Users.Queries;

public class SearchUsersQuery : IRequest<PagedResult<UserDto>>
{
    public string? SearchTerm { get; set; }
    public bool? IsActive { get; set; }
    public Guid? RoleId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

