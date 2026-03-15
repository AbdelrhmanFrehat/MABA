namespace Maba.Application.Features.Orders.DTOs;

public class OrderStatusDto
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? Color { get; set; }
    public int Order { get; set; }
}
