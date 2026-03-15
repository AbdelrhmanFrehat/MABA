using MediatR;
using Maba.Application.Features.Media.DTOs;

namespace Maba.Application.Features.Media.Commands;

public class UpdateMediaCommand : IRequest<MediaAssetDto>
{
    public Guid Id { get; set; }
    public string? TitleEn { get; set; }
    public string? TitleAr { get; set; }
    public string? AltEn { get; set; }
    public string? AltAr { get; set; }
}

