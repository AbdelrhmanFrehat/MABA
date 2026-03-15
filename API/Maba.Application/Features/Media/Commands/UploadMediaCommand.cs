using MediatR;
using Maba.Application.Features.Media.DTOs;
using Microsoft.AspNetCore.Http;

namespace Maba.Application.Features.Media.Commands;

public class UploadMediaCommand : IRequest<MediaAssetDto>
{
    public IFormFile File { get; set; } = null!;
    public Guid MediaTypeId { get; set; }
    public string? TitleEn { get; set; }
    public string? TitleAr { get; set; }
    public string? AltEn { get; set; }
    public string? AltAr { get; set; }
    public Guid? UploadedByUserId { get; set; }
}

