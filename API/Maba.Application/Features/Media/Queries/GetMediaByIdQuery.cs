using MediatR;
using Maba.Application.Features.Media.DTOs;

namespace Maba.Application.Features.Media.Queries;

public class GetMediaByIdQuery : IRequest<MediaAssetDto>
{
    public Guid Id { get; set; }
}

