namespace Maba.Domain.Laser;

public enum LaserServiceRequestStatus
{
    Pending = 0,
    UnderReview = 1,
    Quoted = 2,
    Approved = 3,
    InProgress = 4,
    Completed = 5,
    Cancelled = 6,
    Rejected = 7
}
