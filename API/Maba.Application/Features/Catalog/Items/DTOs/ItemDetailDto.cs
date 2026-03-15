namespace Maba.Application.Features.Catalog.Items.DTOs;

public class ItemDetailDto : ItemDto
{
    public List<ItemSectionDto> Sections { get; set; } = new();
    public List<ReviewDto> Reviews { get; set; } = new();
    public List<CommentDto> Comments { get; set; } = new();
}

public class ItemSectionDto
{
    public Guid Id { get; set; }
    public Guid ItemId { get; set; }
    public string TitleEn { get; set; } = string.Empty;
    public string TitleAr { get; set; } = string.Empty;
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public int SortOrder { get; set; }
    public List<ItemSectionFeatureDto> Features { get; set; } = new();
}

public class ItemSectionFeatureDto
{
    public Guid Id { get; set; }
    public Guid ItemSectionId { get; set; }
    public string TextEn { get; set; } = string.Empty;
    public string TextAr { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

