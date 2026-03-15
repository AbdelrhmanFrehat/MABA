using Maba.Domain.Common;
using Maba.Domain.Users;

namespace Maba.Domain.Finance;

public class Income : BaseEntity
{
    public Guid IncomeSourceId { get; set; }
    public string? RefId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime ReceivedAt { get; set; }
    public Guid EnteredByUserId { get; set; }
    
    // Navigation properties
    public IncomeSource IncomeSource { get; set; } = null!;
    public User EnteredByUser { get; set; } = null!;
}

