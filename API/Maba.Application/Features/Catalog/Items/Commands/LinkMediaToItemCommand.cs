using MediatR;

namespace Maba.Application.Features.Catalog.Items.Commands;

public class LinkMediaToItemCommand : IRequest<ItemMediaDto>
{
    public Guid ItemId { get; set; }
    public Guid MediaAssetId { get; set; }
    public bool IsPrimary { get; set; }
    public int SortOrder { get; set; }
}

public class ItemMediaDto
{
    public Guid Id { get; set; }
    public Guid ItemId { get; set; }
    public Guid MediaAssetId { get; set; }
    public string FileUrl { get; set; } = string.Empty;
    public string? TitleEn { get; set; }
    public string? TitleAr { get; set; }
    public string? AltTextEn { get; set; }
    public string? AltTextAr { get; set; }
    public string? MediaType { get; set; }
    public bool IsPrimary { get; set; }
    public int SortOrder { get; set; }
}
