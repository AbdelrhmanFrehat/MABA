using Maba.Domain.Common;

namespace Maba.Domain.Accounting;

public class FiscalYear : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsClosed { get; set; }
    public Guid? ClosedByUserId { get; set; }
    public DateTime? ClosedAt { get; set; }

    public ICollection<FiscalPeriod> Periods { get; set; } = new List<FiscalPeriod>();
}
