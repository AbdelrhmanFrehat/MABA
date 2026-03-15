namespace Maba.Application.Features.Finance.DTOs;

public class IncomeDto
{
    public Guid Id { get; set; }
    public Guid IncomeSourceId { get; set; }
    public string IncomeSourceKey { get; set; } = string.Empty;
    public string? RefId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime ReceivedAt { get; set; }
    public Guid EnteredByUserId { get; set; }
    public string EnteredByUserFullName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class ExpenseDto
{
    public Guid Id { get; set; }
    public Guid ExpenseCategoryId { get; set; }
    public string ExpenseCategoryKey { get; set; } = string.Empty;
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime SpentAt { get; set; }
    public Guid? ReceiptMediaId { get; set; }
    public Guid EnteredByUserId { get; set; }
    public string EnteredByUserFullName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class IncomeSourceDto
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class ExpenseCategoryDto
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

