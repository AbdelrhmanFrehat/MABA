using MediatR;
using Maba.Application.Features.Media.DTOs;

namespace Maba.Application.Features.Media.Queries;

public class GetAllMediaQuery : IRequest<List<MediaAssetDto>>
{
    public Guid? MediaTypeId { get; set; }
    public Guid? UploadedByUserId { get; set; }
}

