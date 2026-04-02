using MediatR;
using Maba.Application.Common.Models;
using Maba.Application.Features.Printing.DTOs;

namespace Maba.Application.Features.Printing.Designs.Queries;

public class SearchDesignsQuery : IRequest<PagedResult<DesignDto>>
{
    public string? SearchTerm { get; set; }
    public Guid? UserId { get; set; }
    public bool? IsPublic { get; set; }
    public string? LicenseType { get; set; }
    public string? Tags { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; } // Title, CreatedAt, DownloadCount, LikeCount
    public bool SortDescending { get; set; } = true;

    /// <summary>Authenticated user id from JWT (never from client body).</summary>
    public Guid? RequestingUserId { get; set; }
    public bool IsPrivileged { get; set; }
}

