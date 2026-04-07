namespace Maba.Application.Common.ServiceRequests;

public class AdminServiceRequestListItemDto
{
    public Guid Id { get; set; }
    public string RequestType { get; set; } = string.Empty;
    public string ReferenceNumber { get; set; } = string.Empty;
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string WorkflowStatus { get; set; } = ServiceRequestWorkflowStatus.New.ToString();
    public string? LegacyStatus { get; set; }
    public string? Priority { get; set; }
    public DateTime CreatedAt { get; set; }
    public string OriginalModuleRoute { get; set; } = string.Empty;
}

public class AdminServiceRequestHistoryItemDto
{
    public string Status { get; set; } = string.Empty;
    public DateTime? Timestamp { get; set; }
    public bool Reached { get; set; }
}

public class AdminServiceRequestDetailDto : AdminServiceRequestListItemDto
{
    public string? Description { get; set; }
    public string? AdminNotes { get; set; }
    public string? InternalNoteDraft { get; set; }
    public string? RequestTypeName { get; set; }
    public string? FileName { get; set; }
    public string? FileUrl { get; set; }
    public string? DownloadUrl { get; set; }
    public string? ImageUrl { get; set; }
    public decimal? EstimatedPrice { get; set; }
    public decimal? QuotedPrice { get; set; }
    public decimal? FinalPrice { get; set; }
    public string? Currency { get; set; }
    public string? BudgetRange { get; set; }
    public string? Timeline { get; set; }
    public string? DeliveryNotes { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public string? MaterialId { get; set; }
    public string? MaterialName { get; set; }
    public string? MaterialColorId { get; set; }
    public string? MaterialColorName { get; set; }
    public string? ProfileId { get; set; }
    public string? ProfileName { get; set; }
    public decimal? EstimatedPrintTimeHours { get; set; }
    public decimal? SuggestedPrice { get; set; }
    public int? EstimatedFilamentGrams { get; set; }
    public string? UsedSpoolId { get; set; }
    public string? UsedSpoolName { get; set; }

    public string? OperationMode { get; set; }
    public decimal? WidthCm { get; set; }
    public decimal? HeightCm { get; set; }
    public string? CustomerNotes { get; set; }

    public string? ServiceMode { get; set; }
    public string? OperationType { get; set; }
    public decimal? WidthMm { get; set; }
    public decimal? HeightMm { get; set; }
    public decimal? ThicknessMm { get; set; }
    public int? Quantity { get; set; }
    public string? DepthMode { get; set; }
    public decimal? DepthMm { get; set; }
    public string? DesignSourceType { get; set; }
    public string? ProjectDescription { get; set; }
    public string? PcbMaterial { get; set; }
    public decimal? PcbThickness { get; set; }
    public string? PcbSide { get; set; }
    public string? PcbOperation { get; set; }
    public string? DesignNotes { get; set; }

    public string? IntendedUse { get; set; }
    public string? MaterialPreference { get; set; }
    public string? DimensionsNotes { get; set; }
    public string? ToleranceLevel { get; set; }
    public bool? IpOwnershipConfirmed { get; set; }

    public string? TargetProcess { get; set; }
    public string? MaterialNotes { get; set; }
    public string? ToleranceNotes { get; set; }
    public string? WhatNeedsChange { get; set; }
    public string? CriticalSurfaces { get; set; }
    public string? FitmentRequirements { get; set; }
    public string? PurposeAndConstraints { get; set; }
    public string? Deadline { get; set; }
    public bool? HasPhysicalPart { get; set; }
    public bool? LegalConfirmation { get; set; }
    public bool? CanDeliverPhysicalPart { get; set; }

    public string? ProjectId { get; set; }
    public string? ProjectTitle { get; set; }
    public string? Category { get; set; }
    public string? ProjectType { get; set; }
    public string? MainDomain { get; set; }
    public string? ProjectStage { get; set; }
    public List<string> RequiredCapabilities { get; set; } = new();

    public List<AdminServiceRequestHistoryItemDto> History { get; set; } = new();
    public List<AdminServiceRequestAttachmentDto> Attachments { get; set; } = new();
}

public class AdminServiceRequestAttachmentDto
{
    public string Id { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string? Url { get; set; }
    public long? FileSizeBytes { get; set; }
    public DateTime? UploadedAt { get; set; }
}

public class AdminServiceRequestTypeCountDto
{
    public string RequestType { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class AdminServiceRequestStatusCountDto
{
    public string WorkflowStatus { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class AdminServiceRequestsListResponseDto
{
    public List<AdminServiceRequestListItemDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public List<AdminServiceRequestTypeCountDto> TypeCounts { get; set; } = new();
    public List<AdminServiceRequestStatusCountDto> StatusCounts { get; set; } = new();
}

public class UpdateAdminServiceRequestDto
{
    public string? WorkflowStatus { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public string? AdminNotes { get; set; }
    public string? InternalNote { get; set; }
    public string? Priority { get; set; }
    public string? BudgetRange { get; set; }
    public string? Timeline { get; set; }
    public string? DeliveryNotes { get; set; }
    public decimal? EstimatedPrice { get; set; }
    public decimal? QuotedPrice { get; set; }
    public decimal? FinalPrice { get; set; }

    public string? MaterialId { get; set; }
    public string? MaterialColorId { get; set; }
    public string? ProfileId { get; set; }
    public decimal? EstimatedPrintTimeHours { get; set; }
    public decimal? SuggestedPrice { get; set; }
    public int? EstimatedFilamentGrams { get; set; }
    public string? UsedSpoolId { get; set; }

    public string? OperationMode { get; set; }
    public decimal? WidthCm { get; set; }
    public decimal? HeightCm { get; set; }
    public string? CustomerNotes { get; set; }

    public string? ServiceMode { get; set; }
    public string? OperationType { get; set; }
    public decimal? WidthMm { get; set; }
    public decimal? HeightMm { get; set; }
    public decimal? ThicknessMm { get; set; }
    public int? Quantity { get; set; }
    public string? DepthMode { get; set; }
    public decimal? DepthMm { get; set; }
    public string? DesignSourceType { get; set; }
    public string? ProjectDescription { get; set; }
    public string? PcbMaterial { get; set; }
    public decimal? PcbThickness { get; set; }
    public string? PcbSide { get; set; }
    public string? PcbOperation { get; set; }
    public string? DesignNotes { get; set; }

    public string? RequestTypeName { get; set; }
    public string? IntendedUse { get; set; }
    public string? MaterialPreference { get; set; }
    public string? DimensionsNotes { get; set; }
    public string? ToleranceLevel { get; set; }
    public bool? IpOwnershipConfirmed { get; set; }

    public string? TargetProcess { get; set; }
    public string? MaterialNotes { get; set; }
    public string? ToleranceNotes { get; set; }
    public string? WhatNeedsChange { get; set; }
    public string? CriticalSurfaces { get; set; }
    public string? FitmentRequirements { get; set; }
    public string? PurposeAndConstraints { get; set; }
    public string? Deadline { get; set; }
    public bool? HasPhysicalPart { get; set; }
    public bool? LegalConfirmation { get; set; }
    public bool? CanDeliverPhysicalPart { get; set; }

    public string? ProjectId { get; set; }
    public string? Category { get; set; }
    public string? ProjectType { get; set; }
    public string? MainDomain { get; set; }
    public string? ProjectStage { get; set; }
    public List<string>? RequiredCapabilities { get; set; }
}
