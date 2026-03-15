namespace Maba.Domain.Printing;

public enum Print3dServiceRequestStatus
{
    Pending,
    UnderReview,
    Quoted,
    Approved,
    Rejected,
    Queued,
    Slicing,
    Printing,
    Completed,
    Failed,
    Cancelled
}
