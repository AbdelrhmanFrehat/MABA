using MediatR;
using Maba.Application.Features.Catalog.Tags.DTOs;

namespace Maba.Application.Features.Catalog.Tags.Queries;

public class GetPopularTagsQuery : IRequest<List<TagDto>>
{
    public int? Limit { get; set; } = 10;
}

