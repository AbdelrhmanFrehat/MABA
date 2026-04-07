using Maba.Domain.Cnc;
using Maba.Domain.Design;
using Maba.Domain.DesignCad;
using Maba.Domain.Laser;
using Maba.Domain.Printing;
using Maba.Domain.Projects;

namespace Maba.Application.Common.ServiceRequests;

public enum ServiceRequestWorkflowStatus
{
    New = 0,
    UnderReview = 1,
    AwaitingCustomerConfirmation = 2,
    Approved = 3,
    InProgress = 4,
    ReadyForDelivery = 5,
    Completed = 6,
    Rejected = 7,
    Cancelled = 8
}

public static class ServiceRequestWorkflowMapper
{
    public static ServiceRequestWorkflowStatus FromDesign(DesignServiceRequestStatus status) => status switch
    {
        DesignServiceRequestStatus.New => ServiceRequestWorkflowStatus.New,
        DesignServiceRequestStatus.UnderReview => ServiceRequestWorkflowStatus.UnderReview,
        DesignServiceRequestStatus.Quoted => ServiceRequestWorkflowStatus.AwaitingCustomerConfirmation,
        DesignServiceRequestStatus.Approved => ServiceRequestWorkflowStatus.Approved,
        DesignServiceRequestStatus.InProgress => ServiceRequestWorkflowStatus.InProgress,
        DesignServiceRequestStatus.Delivered => ServiceRequestWorkflowStatus.ReadyForDelivery,
        DesignServiceRequestStatus.Closed => ServiceRequestWorkflowStatus.Completed,
        DesignServiceRequestStatus.Rejected => ServiceRequestWorkflowStatus.Rejected,
        DesignServiceRequestStatus.Cancelled => ServiceRequestWorkflowStatus.Cancelled,
        _ => ServiceRequestWorkflowStatus.New
    };

    public static ServiceRequestWorkflowStatus FromDesignCad(DesignCadRequestStatus status) => status switch
    {
        DesignCadRequestStatus.Pending => ServiceRequestWorkflowStatus.New,
        DesignCadRequestStatus.UnderReview => ServiceRequestWorkflowStatus.UnderReview,
        DesignCadRequestStatus.Quoted => ServiceRequestWorkflowStatus.AwaitingCustomerConfirmation,
        DesignCadRequestStatus.Approved => ServiceRequestWorkflowStatus.Approved,
        DesignCadRequestStatus.Completed => ServiceRequestWorkflowStatus.Completed,
        DesignCadRequestStatus.Rejected => ServiceRequestWorkflowStatus.Rejected,
        DesignCadRequestStatus.Cancelled => ServiceRequestWorkflowStatus.Cancelled,
        _ => ServiceRequestWorkflowStatus.New
    };

    public static ServiceRequestWorkflowStatus FromPrint3d(Print3dServiceRequestStatus status) => status switch
    {
        Print3dServiceRequestStatus.Pending => ServiceRequestWorkflowStatus.New,
        Print3dServiceRequestStatus.UnderReview => ServiceRequestWorkflowStatus.UnderReview,
        Print3dServiceRequestStatus.Quoted => ServiceRequestWorkflowStatus.AwaitingCustomerConfirmation,
        Print3dServiceRequestStatus.Approved => ServiceRequestWorkflowStatus.Approved,
        Print3dServiceRequestStatus.Queued => ServiceRequestWorkflowStatus.InProgress,
        Print3dServiceRequestStatus.Slicing => ServiceRequestWorkflowStatus.InProgress,
        Print3dServiceRequestStatus.Printing => ServiceRequestWorkflowStatus.InProgress,
        Print3dServiceRequestStatus.Completed => ServiceRequestWorkflowStatus.Completed,
        Print3dServiceRequestStatus.Failed => ServiceRequestWorkflowStatus.Rejected,
        Print3dServiceRequestStatus.Rejected => ServiceRequestWorkflowStatus.Rejected,
        Print3dServiceRequestStatus.Cancelled => ServiceRequestWorkflowStatus.Cancelled,
        _ => ServiceRequestWorkflowStatus.New
    };

    public static ServiceRequestWorkflowStatus FromLaser(LaserServiceRequestStatus status) => status switch
    {
        LaserServiceRequestStatus.Pending => ServiceRequestWorkflowStatus.New,
        LaserServiceRequestStatus.UnderReview => ServiceRequestWorkflowStatus.UnderReview,
        LaserServiceRequestStatus.Quoted => ServiceRequestWorkflowStatus.AwaitingCustomerConfirmation,
        LaserServiceRequestStatus.Approved => ServiceRequestWorkflowStatus.Approved,
        LaserServiceRequestStatus.InProgress => ServiceRequestWorkflowStatus.InProgress,
        LaserServiceRequestStatus.Completed => ServiceRequestWorkflowStatus.Completed,
        LaserServiceRequestStatus.Rejected => ServiceRequestWorkflowStatus.Rejected,
        LaserServiceRequestStatus.Cancelled => ServiceRequestWorkflowStatus.Cancelled,
        _ => ServiceRequestWorkflowStatus.New
    };

    public static ServiceRequestWorkflowStatus FromCnc(CncServiceRequestStatus status) => status switch
    {
        CncServiceRequestStatus.Pending => ServiceRequestWorkflowStatus.New,
        CncServiceRequestStatus.InReview => ServiceRequestWorkflowStatus.UnderReview,
        CncServiceRequestStatus.Quoted => ServiceRequestWorkflowStatus.AwaitingCustomerConfirmation,
        CncServiceRequestStatus.Accepted => ServiceRequestWorkflowStatus.Approved,
        CncServiceRequestStatus.InProgress => ServiceRequestWorkflowStatus.InProgress,
        CncServiceRequestStatus.Completed => ServiceRequestWorkflowStatus.Completed,
        CncServiceRequestStatus.Rejected => ServiceRequestWorkflowStatus.Rejected,
        CncServiceRequestStatus.Cancelled => ServiceRequestWorkflowStatus.Cancelled,
        _ => ServiceRequestWorkflowStatus.New
    };

    public static ServiceRequestWorkflowStatus FromProject(ProjectRequestStatus status) => status switch
    {
        ProjectRequestStatus.New => ServiceRequestWorkflowStatus.New,
        ProjectRequestStatus.InReview => ServiceRequestWorkflowStatus.UnderReview,
        ProjectRequestStatus.Quoted => ServiceRequestWorkflowStatus.AwaitingCustomerConfirmation,
        ProjectRequestStatus.InProgress => ServiceRequestWorkflowStatus.InProgress,
        ProjectRequestStatus.Closed => ServiceRequestWorkflowStatus.Completed,
        _ => ServiceRequestWorkflowStatus.New
    };

    public static DesignServiceRequestStatus ToDesign(ServiceRequestWorkflowStatus status) => status switch
    {
        ServiceRequestWorkflowStatus.New => DesignServiceRequestStatus.New,
        ServiceRequestWorkflowStatus.UnderReview => DesignServiceRequestStatus.UnderReview,
        ServiceRequestWorkflowStatus.AwaitingCustomerConfirmation => DesignServiceRequestStatus.Quoted,
        ServiceRequestWorkflowStatus.Approved => DesignServiceRequestStatus.Approved,
        ServiceRequestWorkflowStatus.InProgress => DesignServiceRequestStatus.InProgress,
        ServiceRequestWorkflowStatus.ReadyForDelivery => DesignServiceRequestStatus.Delivered,
        ServiceRequestWorkflowStatus.Completed => DesignServiceRequestStatus.Closed,
        ServiceRequestWorkflowStatus.Rejected => DesignServiceRequestStatus.Rejected,
        ServiceRequestWorkflowStatus.Cancelled => DesignServiceRequestStatus.Cancelled,
        _ => DesignServiceRequestStatus.New
    };

    public static DesignCadRequestStatus ToDesignCad(ServiceRequestWorkflowStatus status) => status switch
    {
        ServiceRequestWorkflowStatus.New => DesignCadRequestStatus.Pending,
        ServiceRequestWorkflowStatus.UnderReview => DesignCadRequestStatus.UnderReview,
        ServiceRequestWorkflowStatus.AwaitingCustomerConfirmation => DesignCadRequestStatus.Quoted,
        ServiceRequestWorkflowStatus.Approved => DesignCadRequestStatus.Approved,
        ServiceRequestWorkflowStatus.InProgress => DesignCadRequestStatus.Approved,
        ServiceRequestWorkflowStatus.ReadyForDelivery => DesignCadRequestStatus.Approved,
        ServiceRequestWorkflowStatus.Completed => DesignCadRequestStatus.Completed,
        ServiceRequestWorkflowStatus.Rejected => DesignCadRequestStatus.Rejected,
        ServiceRequestWorkflowStatus.Cancelled => DesignCadRequestStatus.Cancelled,
        _ => DesignCadRequestStatus.Pending
    };

    public static Print3dServiceRequestStatus ToPrint3d(ServiceRequestWorkflowStatus status) => status switch
    {
        ServiceRequestWorkflowStatus.New => Print3dServiceRequestStatus.Pending,
        ServiceRequestWorkflowStatus.UnderReview => Print3dServiceRequestStatus.UnderReview,
        ServiceRequestWorkflowStatus.AwaitingCustomerConfirmation => Print3dServiceRequestStatus.Quoted,
        ServiceRequestWorkflowStatus.Approved => Print3dServiceRequestStatus.Approved,
        ServiceRequestWorkflowStatus.InProgress => Print3dServiceRequestStatus.Printing,
        ServiceRequestWorkflowStatus.ReadyForDelivery => Print3dServiceRequestStatus.Printing,
        ServiceRequestWorkflowStatus.Completed => Print3dServiceRequestStatus.Completed,
        ServiceRequestWorkflowStatus.Rejected => Print3dServiceRequestStatus.Rejected,
        ServiceRequestWorkflowStatus.Cancelled => Print3dServiceRequestStatus.Cancelled,
        _ => Print3dServiceRequestStatus.Pending
    };

    public static LaserServiceRequestStatus ToLaser(ServiceRequestWorkflowStatus status) => status switch
    {
        ServiceRequestWorkflowStatus.New => LaserServiceRequestStatus.Pending,
        ServiceRequestWorkflowStatus.UnderReview => LaserServiceRequestStatus.UnderReview,
        ServiceRequestWorkflowStatus.AwaitingCustomerConfirmation => LaserServiceRequestStatus.Quoted,
        ServiceRequestWorkflowStatus.Approved => LaserServiceRequestStatus.Approved,
        ServiceRequestWorkflowStatus.InProgress => LaserServiceRequestStatus.InProgress,
        ServiceRequestWorkflowStatus.ReadyForDelivery => LaserServiceRequestStatus.InProgress,
        ServiceRequestWorkflowStatus.Completed => LaserServiceRequestStatus.Completed,
        ServiceRequestWorkflowStatus.Rejected => LaserServiceRequestStatus.Rejected,
        ServiceRequestWorkflowStatus.Cancelled => LaserServiceRequestStatus.Cancelled,
        _ => LaserServiceRequestStatus.Pending
    };

    public static CncServiceRequestStatus ToCnc(ServiceRequestWorkflowStatus status) => status switch
    {
        ServiceRequestWorkflowStatus.New => CncServiceRequestStatus.Pending,
        ServiceRequestWorkflowStatus.UnderReview => CncServiceRequestStatus.InReview,
        ServiceRequestWorkflowStatus.AwaitingCustomerConfirmation => CncServiceRequestStatus.Quoted,
        ServiceRequestWorkflowStatus.Approved => CncServiceRequestStatus.Accepted,
        ServiceRequestWorkflowStatus.InProgress => CncServiceRequestStatus.InProgress,
        ServiceRequestWorkflowStatus.ReadyForDelivery => CncServiceRequestStatus.InProgress,
        ServiceRequestWorkflowStatus.Completed => CncServiceRequestStatus.Completed,
        ServiceRequestWorkflowStatus.Rejected => CncServiceRequestStatus.Rejected,
        ServiceRequestWorkflowStatus.Cancelled => CncServiceRequestStatus.Cancelled,
        _ => CncServiceRequestStatus.Pending
    };

    public static ProjectRequestStatus ToProject(ServiceRequestWorkflowStatus status) => status switch
    {
        ServiceRequestWorkflowStatus.New => ProjectRequestStatus.New,
        ServiceRequestWorkflowStatus.UnderReview => ProjectRequestStatus.InReview,
        ServiceRequestWorkflowStatus.AwaitingCustomerConfirmation => ProjectRequestStatus.Quoted,
        ServiceRequestWorkflowStatus.Approved => ProjectRequestStatus.InProgress,
        ServiceRequestWorkflowStatus.InProgress => ProjectRequestStatus.InProgress,
        ServiceRequestWorkflowStatus.ReadyForDelivery => ProjectRequestStatus.InProgress,
        ServiceRequestWorkflowStatus.Completed => ProjectRequestStatus.Closed,
        ServiceRequestWorkflowStatus.Rejected => ProjectRequestStatus.Closed,
        ServiceRequestWorkflowStatus.Cancelled => ProjectRequestStatus.Closed,
        _ => ProjectRequestStatus.New
    };
}
