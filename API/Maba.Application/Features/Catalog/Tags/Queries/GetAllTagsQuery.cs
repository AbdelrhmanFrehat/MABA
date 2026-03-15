using MediatR;
using Maba.Application.Features.Catalog.Tags.DTOs;

namespace Maba.Application.Features.Catalog.Tags.Queries;

public class GetAllTagsQuery : IRequest<List<TagDto>>
{
    public bool? IsActive { get; set; }
}

