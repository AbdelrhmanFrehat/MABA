using MediatR;
using Maba.Application.Features.Catalog.Tags.DTOs;

namespace Maba.Application.Features.Catalog.Tags.Commands;

public class UpdateTagCommand : IRequest<TagDto>
{
    public Guid Id { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

