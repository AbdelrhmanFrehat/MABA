namespace Maba.Application.Features.Catalog.Items.DTOs;

public class ItemStatusDto
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
}
