using MediatR;
using Maba.Application.Features.Catalog.Tags.DTOs;

namespace Maba.Application.Features.Catalog.Tags.Queries;

public class GetTagByIdQuery : IRequest<TagDto>
{
    public Guid Id { get; set; }
}

