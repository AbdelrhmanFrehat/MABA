namespace Maba.Application.Features.Crm.DTOs;

public class CustomerDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public Guid CustomerTypeId { get; set; }
    public decimal CreditLimit { get; set; }
    public decimal CurrentBalance { get; set; }
    public string Currency { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class SupplierDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public Guid? SupplierTypeId { get; set; }
    public decimal CreditLimit { get; set; }
    public decimal CurrentBalance { get; set; }
    public string Currency { get; set; } = string.Empty;
    public int PaymentTermDays { get; set; }
    public bool IsActive { get; set; }
}
