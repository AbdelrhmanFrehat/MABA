using MediatR;
using Maba.Application.Features.Catalog.Tags.DTOs;

namespace Maba.Application.Features.Catalog.Tags.Queries;

public class SearchTagsQuery : IRequest<List<TagDto>>
{
    public string? SearchTerm { get; set; }
    public bool? IsActive { get; set; }
}

