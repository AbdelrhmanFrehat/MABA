using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Common.ServiceRequests;
using Maba.Application.Features.Projects;
using Maba.Domain.Cnc;
using Maba.Domain.Design;
using Maba.Domain.DesignCad;
using Maba.Domain.Laser;
using Maba.Domain.Printing;
using Maba.Domain.Projects;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/all-requests")]
[Authorize(Roles = "Admin,Manager")]
public class AllRequestsController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public AllRequestsController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<AdminServiceRequestsListResponseDto>> GetAll(
        [FromQuery] string? requestType = null,
        [FromQuery] string? workflowStatus = null,
        [FromQuery] DateTime? createdFrom = null,
        [FromQuery] DateTime? createdTo = null,
        [FromQuery] string? keyword = null,
        [FromQuery] string? customer = null,
        [FromQuery] string? reference = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 5, 100);

        var normalizedType = NormalizeRequestType(requestType);
        var normalizedWorkflow = ParseWorkflowStatus(workflowStatus);
        var items = new List<AdminServiceRequestListItemDto>();

        if (normalizedType is null or "project")
            items.AddRange(await GetProjectItems(createdFrom, createdTo, keyword, customer, reference));
        if (normalizedType is null or "print3d")
            items.AddRange(await GetPrint3dItems(createdFrom, createdTo, keyword, customer, reference));
        if (normalizedType is null or "design")
            items.AddRange(await GetDesignItems(createdFrom, createdTo, keyword, customer, reference));
        if (normalizedType is null or "designCad")
            items.AddRange(await GetDesignCadItems(createdFrom, createdTo, keyword, customer, reference));
        if (normalizedType is null or "laser")
            items.AddRange(await GetLaserItems(createdFrom, createdTo, keyword, customer, reference));
        if (normalizedType is null or "cnc")
            items.AddRange(await GetCncItems(createdFrom, createdTo, keyword, customer, reference));

        if (normalizedWorkflow.HasValue)
        {
            var workflowKey = normalizedWorkflow.Value.ToString();
            items = items.Where(x => x.WorkflowStatus == workflowKey).ToList();
        }

        items = items.OrderByDescending(x => x.CreatedAt).ToList();

        var typeCounts = items
            .GroupBy(x => x.RequestType)
            .Select(g => new AdminServiceRequestTypeCountDto { RequestType = g.Key, Count = g.Count() })
            .OrderBy(x => x.RequestType)
            .ToList();

        var statusCounts = items
            .GroupBy(x => x.WorkflowStatus)
            .Select(g => new AdminServiceRequestStatusCountDto { WorkflowStatus = g.Key, Count = g.Count() })
            .OrderBy(x => x.WorkflowStatus)
            .ToList();

        var totalCount = items.Count;
        var paged = items.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return Ok(new AdminServiceRequestsListResponseDto
        {
            Items = paged,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TypeCounts = typeCounts,
            StatusCounts = statusCounts
        });
    }

    [HttpGet("{requestType}/{id:guid}")]
    public async Task<ActionResult<AdminServiceRequestDetailDto>> GetDetail(string requestType, Guid id)
    {
        var normalizedType = NormalizeRequestType(requestType);
        if (normalizedType == null)
            return NotFound();

        var dto = normalizedType switch
        {
            "project" => await GetProjectDetail(id),
            "print3d" => await GetPrint3dDetail(id),
            "design" => await GetDesignDetail(id),
            "designCad" => await GetDesignCadDetail(id),
            "laser" => await GetLaserDetail(id),
            "cnc" => await GetCncDetail(id),
            _ => null
        };

        return dto == null ? NotFound() : Ok(dto);
    }

    [HttpPut("{requestType}/{id:guid}")]
    public async Task<ActionResult<AdminServiceRequestDetailDto>> Update(
        string requestType,
        Guid id,
        [FromBody] UpdateAdminServiceRequestDto dto)
    {
        var normalizedType = NormalizeRequestType(requestType);
        if (normalizedType == null)
            return NotFound();

        switch (normalizedType)
        {
            case "project":
            {
                var entity = await _context.Set<ProjectRequest>().FirstOrDefaultAsync(x => x.Id == id);
                if (entity == null) return NotFound();

                entity.FullName = Coalesce(dto.CustomerName, entity.FullName);
                entity.Email = Coalesce(dto.CustomerEmail, entity.Email);
                entity.Phone = dto.CustomerPhone ?? entity.Phone;
                entity.BudgetRange = dto.BudgetRange ?? entity.BudgetRange;
                entity.Timeline = dto.Timeline ?? entity.Timeline;
                entity.Description = dto.Description ?? entity.Description;
                entity.AdminNotes = MergeNotes(dto.AdminNotes, dto.InternalNote, entity.AdminNotes);
                entity.ProjectType = dto.ProjectType ?? entity.ProjectType;
                entity.MainDomain = dto.MainDomain ?? entity.MainDomain;
                entity.ProjectStage = dto.ProjectStage ?? entity.ProjectStage;
                if (dto.RequiredCapabilities != null)
                    entity.RequiredCapabilitiesJson = ProjectRequestSerialization.SerializeCapabilities(dto.RequiredCapabilities);
                if (Enum.TryParse<ProjectRequestType>(dto.RequestTypeName, true, out var requestTypeEnum))
                    entity.RequestType = requestTypeEnum;
                if (Enum.TryParse<ProjectCategory>(dto.Category, true, out var categoryEnum))
                    entity.Category = categoryEnum;
                if (Guid.TryParse(dto.ProjectId, out var projectId))
                    entity.ProjectId = projectId;
                if (ParseWorkflowStatus(dto.WorkflowStatus) is { } projectWorkflowStatus)
                    entity.Status = ServiceRequestWorkflowMapper.ToProject(projectWorkflowStatus);
                break;
            }
            case "print3d":
            {
                var entity = await _context.Set<Print3dServiceRequest>().FirstOrDefaultAsync(x => x.Id == id);
                if (entity == null) return NotFound();

                entity.CustomerName = dto.CustomerName ?? entity.CustomerName;
                entity.CustomerEmail = dto.CustomerEmail ?? entity.CustomerEmail;
                entity.CustomerPhone = dto.CustomerPhone ?? entity.CustomerPhone;
                entity.CustomerNotes = dto.CustomerNotes ?? entity.CustomerNotes;
                entity.AdminNotes = MergeNotes(dto.AdminNotes, dto.InternalNote, entity.AdminNotes);
                entity.EstimatedPrice = dto.EstimatedPrice ?? entity.EstimatedPrice;
                entity.FinalPrice = dto.FinalPrice ?? entity.FinalPrice;
                entity.EstimatedPrintTimeHours = dto.EstimatedPrintTimeHours ?? entity.EstimatedPrintTimeHours;
                entity.SuggestedPrice = dto.SuggestedPrice ?? entity.SuggestedPrice;
                entity.EstimatedFilamentGrams = dto.EstimatedFilamentGrams ?? entity.EstimatedFilamentGrams;
                entity.MaterialId = ParseGuidOrKeep(dto.MaterialId, entity.MaterialId);
                entity.MaterialColorId = ParseNullableGuidOrKeep(dto.MaterialColorId, entity.MaterialColorId);
                entity.ProfileId = ParseNullableGuidOrKeep(dto.ProfileId, entity.ProfileId);
                entity.UsedSpoolId = ParseNullableGuidOrKeep(dto.UsedSpoolId, entity.UsedSpoolId);
                if (ParseWorkflowStatus(dto.WorkflowStatus) is { } printWorkflowStatus)
                {
                    var newStatus = ServiceRequestWorkflowMapper.ToPrint3d(printWorkflowStatus);
                    entity.Status = newStatus;
                    if (newStatus == Print3dServiceRequestStatus.UnderReview && !entity.ReviewedAt.HasValue) entity.ReviewedAt = DateTime.UtcNow;
                    if (newStatus == Print3dServiceRequestStatus.Approved && !entity.ApprovedAt.HasValue) entity.ApprovedAt = DateTime.UtcNow;
                    if (newStatus == Print3dServiceRequestStatus.Completed && !entity.CompletedAt.HasValue) entity.CompletedAt = DateTime.UtcNow;
                }
                break;
            }
            case "design":
            {
                var entity = await _context.Set<DesignServiceRequest>().FirstOrDefaultAsync(x => x.Id == id);
                if (entity == null) return NotFound();

                entity.Title = Coalesce(dto.Title, entity.Title);
                entity.Description = dto.Description ?? entity.Description;
                entity.CustomerName = dto.CustomerName ?? entity.CustomerName;
                entity.CustomerEmail = dto.CustomerEmail ?? entity.CustomerEmail;
                entity.CustomerPhone = dto.CustomerPhone ?? entity.CustomerPhone;
                entity.IntendedUse = dto.IntendedUse ?? entity.IntendedUse;
                entity.MaterialPreference = dto.MaterialPreference ?? entity.MaterialPreference;
                entity.DimensionsNotes = dto.DimensionsNotes ?? entity.DimensionsNotes;
                entity.BudgetRange = dto.BudgetRange ?? entity.BudgetRange;
                entity.Timeline = dto.Timeline ?? entity.Timeline;
                entity.AdminNotes = MergeNotes(dto.AdminNotes, dto.InternalNote, entity.AdminNotes);
                entity.DeliveryNotes = dto.DeliveryNotes ?? entity.DeliveryNotes;
                entity.QuotedPrice = dto.QuotedPrice ?? entity.QuotedPrice;
                entity.FinalPrice = dto.FinalPrice ?? entity.FinalPrice;
                if (dto.IpOwnershipConfirmed.HasValue) entity.IpOwnershipConfirmed = dto.IpOwnershipConfirmed.Value;
                if (Enum.TryParse<DesignServiceRequestType>(dto.RequestTypeName, true, out var designType))
                    entity.RequestType = designType;
                if (Enum.TryParse<ToleranceLevel>(dto.ToleranceLevel, true, out var tolerance))
                    entity.ToleranceLevel = tolerance;
                if (ParseWorkflowStatus(dto.WorkflowStatus) is { } designWorkflowStatus)
                {
                    var newStatus = ServiceRequestWorkflowMapper.ToDesign(designWorkflowStatus);
                    entity.Status = newStatus;
                    if (newStatus == DesignServiceRequestStatus.UnderReview && !entity.ReviewedAt.HasValue) entity.ReviewedAt = DateTime.UtcNow;
                    if (newStatus == DesignServiceRequestStatus.Quoted && !entity.QuotedAt.HasValue) entity.QuotedAt = DateTime.UtcNow;
                    if ((newStatus == DesignServiceRequestStatus.Delivered || newStatus == DesignServiceRequestStatus.Closed) && !entity.DeliveredAt.HasValue) entity.DeliveredAt = DateTime.UtcNow;
                }
                break;
            }
            case "designCad":
            {
                var entity = await _context.Set<DesignCadServiceRequest>().FirstOrDefaultAsync(x => x.Id == id);
                if (entity == null) return NotFound();

                entity.Title = Coalesce(dto.Title, entity.Title);
                entity.Description = dto.Description ?? entity.Description;
                entity.CustomerName = dto.CustomerName ?? entity.CustomerName;
                entity.CustomerEmail = dto.CustomerEmail ?? entity.CustomerEmail;
                entity.CustomerPhone = dto.CustomerPhone ?? entity.CustomerPhone;
                entity.CustomerNotes = dto.CustomerNotes ?? entity.CustomerNotes;
                entity.IntendedUse = dto.IntendedUse ?? entity.IntendedUse;
                entity.MaterialNotes = dto.MaterialNotes ?? entity.MaterialNotes;
                entity.DimensionsNotes = dto.DimensionsNotes ?? entity.DimensionsNotes;
                entity.ToleranceNotes = dto.ToleranceNotes ?? entity.ToleranceNotes;
                entity.WhatNeedsChange = dto.WhatNeedsChange ?? entity.WhatNeedsChange;
                entity.CriticalSurfaces = dto.CriticalSurfaces ?? entity.CriticalSurfaces;
                entity.FitmentRequirements = dto.FitmentRequirements ?? entity.FitmentRequirements;
                entity.PurposeAndConstraints = dto.PurposeAndConstraints ?? entity.PurposeAndConstraints;
                entity.Deadline = dto.Deadline ?? entity.Deadline;
                entity.AdminNotes = MergeNotes(dto.AdminNotes, dto.InternalNote, entity.AdminNotes);
                entity.QuotedPrice = dto.QuotedPrice ?? entity.QuotedPrice;
                entity.FinalPrice = dto.FinalPrice ?? entity.FinalPrice;
                if (dto.HasPhysicalPart.HasValue) entity.HasPhysicalPart = dto.HasPhysicalPart.Value;
                if (dto.LegalConfirmation.HasValue) entity.LegalConfirmation = dto.LegalConfirmation.Value;
                if (dto.CanDeliverPhysicalPart.HasValue) entity.CanDeliverPhysicalPart = dto.CanDeliverPhysicalPart.Value;
                if (Enum.TryParse<DesignCadRequestType>(dto.RequestTypeName, true, out var cadType))
                    entity.RequestType = cadType;
                if (Enum.TryParse<DesignCadTargetProcess>(dto.TargetProcess, true, out var targetProcess))
                    entity.TargetProcess = targetProcess;
                if (ParseWorkflowStatus(dto.WorkflowStatus) is { } cadWorkflowStatus)
                {
                    var newStatus = ServiceRequestWorkflowMapper.ToDesignCad(cadWorkflowStatus);
                    entity.Status = newStatus;
                    if (newStatus == DesignCadRequestStatus.UnderReview && !entity.ReviewedAt.HasValue) entity.ReviewedAt = DateTime.UtcNow;
                    if (newStatus == DesignCadRequestStatus.Completed && !entity.CompletedAt.HasValue) entity.CompletedAt = DateTime.UtcNow;
                }
                break;
            }
            case "laser":
            {
                var entity = await _context.Set<LaserServiceRequest>().FirstOrDefaultAsync(x => x.Id == id);
                if (entity == null) return NotFound();

                entity.CustomerName = dto.CustomerName ?? entity.CustomerName;
                entity.CustomerEmail = dto.CustomerEmail ?? entity.CustomerEmail;
                entity.CustomerPhone = dto.CustomerPhone ?? entity.CustomerPhone;
                entity.CustomerNotes = dto.CustomerNotes ?? entity.CustomerNotes;
                entity.AdminNotes = MergeNotes(dto.AdminNotes, dto.InternalNote, entity.AdminNotes);
                entity.QuotedPrice = dto.QuotedPrice ?? entity.QuotedPrice;
                entity.WidthCm = dto.WidthCm ?? entity.WidthCm;
                entity.HeightCm = dto.HeightCm ?? entity.HeightCm;
                entity.OperationMode = dto.OperationMode ?? entity.OperationMode;
                entity.MaterialId = ParseGuidOrKeep(dto.MaterialId, entity.MaterialId);
                if (ParseWorkflowStatus(dto.WorkflowStatus) is { } laserWorkflowStatus)
                {
                    var newStatus = ServiceRequestWorkflowMapper.ToLaser(laserWorkflowStatus);
                    entity.Status = newStatus;
                    if (newStatus == LaserServiceRequestStatus.UnderReview && !entity.ReviewedAt.HasValue) entity.ReviewedAt = DateTime.UtcNow;
                    if (newStatus == LaserServiceRequestStatus.Completed && !entity.CompletedAt.HasValue) entity.CompletedAt = DateTime.UtcNow;
                }
                break;
            }
            case "cnc":
            {
                var entity = await _context.Set<CncServiceRequest>().FirstOrDefaultAsync(x => x.Id == id);
                if (entity == null) return NotFound();

                entity.CustomerName = Coalesce(dto.CustomerName, entity.CustomerName);
                entity.CustomerEmail = Coalesce(dto.CustomerEmail, entity.CustomerEmail);
                entity.CustomerPhone = dto.CustomerPhone ?? entity.CustomerPhone;
                entity.AdminNotes = MergeNotes(dto.AdminNotes, dto.InternalNote, entity.AdminNotes);
                entity.ServiceMode = dto.ServiceMode ?? entity.ServiceMode;
                entity.MaterialId = ParseNullableGuidOrKeep(dto.MaterialId, entity.MaterialId);
                entity.OperationType = dto.OperationType ?? entity.OperationType;
                entity.WidthMm = dto.WidthMm ?? entity.WidthMm;
                entity.HeightMm = dto.HeightMm ?? entity.HeightMm;
                entity.ThicknessMm = dto.ThicknessMm ?? entity.ThicknessMm;
                entity.Quantity = dto.Quantity ?? entity.Quantity;
                entity.DepthMode = dto.DepthMode ?? entity.DepthMode;
                entity.DepthMm = dto.DepthMm ?? entity.DepthMm;
                entity.DesignSourceType = dto.DesignSourceType ?? entity.DesignSourceType;
                entity.DesignNotes = dto.DesignNotes ?? entity.DesignNotes;
                entity.ProjectDescription = dto.ProjectDescription ?? entity.ProjectDescription;
                entity.PcbMaterial = dto.PcbMaterial ?? entity.PcbMaterial;
                entity.PcbThickness = dto.PcbThickness ?? entity.PcbThickness;
                entity.PcbSide = dto.PcbSide ?? entity.PcbSide;
                entity.PcbOperation = dto.PcbOperation ?? entity.PcbOperation;
                entity.EstimatedPrice = dto.EstimatedPrice ?? entity.EstimatedPrice;
                entity.FinalPrice = dto.FinalPrice ?? entity.FinalPrice;
                if (ParseWorkflowStatus(dto.WorkflowStatus) is { } cncWorkflowStatus)
                {
                    var newStatus = ServiceRequestWorkflowMapper.ToCnc(cncWorkflowStatus);
                    entity.Status = newStatus;
                    if (newStatus == CncServiceRequestStatus.InReview && !entity.ReviewedAt.HasValue) entity.ReviewedAt = DateTime.UtcNow;
                    if (newStatus == CncServiceRequestStatus.Completed && !entity.CompletedAt.HasValue) entity.CompletedAt = DateTime.UtcNow;
                }
                break;
            }
        }

        await _context.SaveChangesAsync(CancellationToken.None);
        return await GetDetail(requestType, id);
    }

    private async Task<List<AdminServiceRequestListItemDto>> GetProjectItems(DateTime? createdFrom, DateTime? createdTo, string? keyword, string? customer, string? reference)
    {
        var query = _context.Set<ProjectRequest>().AsNoTracking().Include(x => x.Project).AsQueryable();
        query = ApplyDateFilter(query, createdFrom, createdTo);
        if (!string.IsNullOrWhiteSpace(reference)) query = query.Where(x => x.ReferenceNumber.Contains(reference));
        if (!string.IsNullOrWhiteSpace(customer)) query = query.Where(x => x.FullName.Contains(customer) || x.Email.Contains(customer) || (x.Phone != null && x.Phone.Contains(customer)));
        if (!string.IsNullOrWhiteSpace(keyword)) query = query.Where(x => x.ReferenceNumber.Contains(keyword) || x.FullName.Contains(keyword) || x.Email.Contains(keyword) || (x.Description != null && x.Description.Contains(keyword)) || (x.Project != null && x.Project.TitleEn.Contains(keyword)));

        return await query.Select(x => new AdminServiceRequestListItemDto
        {
            Id = x.Id,
            RequestType = "project",
            ReferenceNumber = x.ReferenceNumber,
            CustomerName = x.FullName,
            CustomerEmail = x.Email,
            CustomerPhone = x.Phone,
            Title = x.Project != null
                ? x.Project.TitleEn
                : (!string.IsNullOrWhiteSpace(x.MainDomain) ? x.MainDomain : (x.Category != null ? x.Category.ToString()! : "Project Request")),
            Summary = x.Description,
            WorkflowStatus = ServiceRequestWorkflowMapper.FromProject(x.Status).ToString(),
            LegacyStatus = x.Status.ToString(),
            CreatedAt = x.CreatedAt,
            Priority = x.ProjectStage,
            OriginalModuleRoute = "/admin/project-requests"
        }).ToListAsync();
    }

    private async Task<List<AdminServiceRequestListItemDto>> GetPrint3dItems(DateTime? createdFrom, DateTime? createdTo, string? keyword, string? customer, string? reference)
    {
        var query = _context.Set<Print3dServiceRequest>().AsNoTracking().Include(x => x.Material).Include(x => x.User).AsQueryable();
        query = ApplyDateFilter(query, createdFrom, createdTo);
        if (!string.IsNullOrWhiteSpace(reference)) query = query.Where(x => x.ReferenceNumber.Contains(reference));
        if (!string.IsNullOrWhiteSpace(customer)) query = query.Where(x => (x.CustomerName != null && x.CustomerName.Contains(customer)) || (x.CustomerEmail != null && x.CustomerEmail.Contains(customer)) || (x.CustomerPhone != null && x.CustomerPhone.Contains(customer)) || (x.User != null && x.User.FullName.Contains(customer)) || (x.User != null && x.User.Email.Contains(customer)));
        if (!string.IsNullOrWhiteSpace(keyword)) query = query.Where(x => x.ReferenceNumber.Contains(keyword) || x.FileName.Contains(keyword) || (x.CustomerNotes != null && x.CustomerNotes.Contains(keyword)) || (x.Material != null && x.Material.NameEn.Contains(keyword)));

        return await query.Select(x => new AdminServiceRequestListItemDto
        {
            Id = x.Id,
            RequestType = "print3d",
            ReferenceNumber = x.ReferenceNumber,
            CustomerName = x.CustomerName ?? (x.User != null ? x.User.FullName : null),
            CustomerEmail = x.CustomerEmail ?? (x.User != null ? x.User.Email : null),
            CustomerPhone = x.CustomerPhone,
            Title = x.FileName,
            Summary = x.CustomerNotes,
            WorkflowStatus = ServiceRequestWorkflowMapper.FromPrint3d(x.Status).ToString(),
            LegacyStatus = x.Status.ToString(),
            CreatedAt = x.CreatedAt,
            OriginalModuleRoute = "/admin/3d-requests"
        }).ToListAsync();
    }

    private async Task<List<AdminServiceRequestListItemDto>> GetDesignItems(DateTime? createdFrom, DateTime? createdTo, string? keyword, string? customer, string? reference)
    {
        var query = _context.Set<DesignServiceRequest>().AsNoTracking().Include(x => x.User).AsQueryable();
        query = ApplyDateFilter(query, createdFrom, createdTo);
        if (!string.IsNullOrWhiteSpace(reference)) query = query.Where(x => x.ReferenceNumber.Contains(reference));
        if (!string.IsNullOrWhiteSpace(customer)) query = query.Where(x => (x.CustomerName != null && x.CustomerName.Contains(customer)) || (x.CustomerEmail != null && x.CustomerEmail.Contains(customer)) || (x.CustomerPhone != null && x.CustomerPhone.Contains(customer)) || (x.User != null && x.User.FullName.Contains(customer)) || (x.User != null && x.User.Email.Contains(customer)));
        if (!string.IsNullOrWhiteSpace(keyword)) query = query.Where(x => x.ReferenceNumber.Contains(keyword) || x.Title.Contains(keyword) || (x.Description != null && x.Description.Contains(keyword)));

        return await query.Select(x => new AdminServiceRequestListItemDto
        {
            Id = x.Id,
            RequestType = "design",
            ReferenceNumber = x.ReferenceNumber,
            CustomerName = x.CustomerName ?? (x.User != null ? x.User.FullName : null),
            CustomerEmail = x.CustomerEmail ?? (x.User != null ? x.User.Email : null),
            CustomerPhone = x.CustomerPhone,
            Title = x.Title,
            Summary = x.Description,
            WorkflowStatus = ServiceRequestWorkflowMapper.FromDesign(x.Status).ToString(),
            LegacyStatus = x.Status.ToString(),
            CreatedAt = x.CreatedAt,
            OriginalModuleRoute = "/admin/design-requests"
        }).ToListAsync();
    }

    private async Task<List<AdminServiceRequestListItemDto>> GetDesignCadItems(DateTime? createdFrom, DateTime? createdTo, string? keyword, string? customer, string? reference)
    {
        var query = _context.Set<DesignCadServiceRequest>().AsNoTracking().Include(x => x.User).AsQueryable();
        query = ApplyDateFilter(query, createdFrom, createdTo);
        if (!string.IsNullOrWhiteSpace(reference)) query = query.Where(x => x.ReferenceNumber.Contains(reference));
        if (!string.IsNullOrWhiteSpace(customer)) query = query.Where(x => (x.CustomerName != null && x.CustomerName.Contains(customer)) || (x.CustomerEmail != null && x.CustomerEmail.Contains(customer)) || (x.CustomerPhone != null && x.CustomerPhone.Contains(customer)) || (x.User != null && x.User.FullName.Contains(customer)) || (x.User != null && x.User.Email.Contains(customer)));
        if (!string.IsNullOrWhiteSpace(keyword)) query = query.Where(x => x.ReferenceNumber.Contains(keyword) || x.Title.Contains(keyword) || (x.Description != null && x.Description.Contains(keyword)));

        return await query.Select(x => new AdminServiceRequestListItemDto
        {
            Id = x.Id,
            RequestType = "designCad",
            ReferenceNumber = x.ReferenceNumber,
            CustomerName = x.CustomerName ?? (x.User != null ? x.User.FullName : null),
            CustomerEmail = x.CustomerEmail ?? (x.User != null ? x.User.Email : null),
            CustomerPhone = x.CustomerPhone,
            Title = x.Title,
            Summary = x.Description,
            WorkflowStatus = ServiceRequestWorkflowMapper.FromDesignCad(x.Status).ToString(),
            LegacyStatus = x.Status.ToString(),
            CreatedAt = x.CreatedAt,
            OriginalModuleRoute = "/admin/cad-requests"
        }).ToListAsync();
    }

    private async Task<List<AdminServiceRequestListItemDto>> GetLaserItems(DateTime? createdFrom, DateTime? createdTo, string? keyword, string? customer, string? reference)
    {
        var query = _context.Set<LaserServiceRequest>().AsNoTracking().Include(x => x.Material).AsQueryable();
        query = ApplyDateFilter(query, createdFrom, createdTo);
        if (!string.IsNullOrWhiteSpace(reference)) query = query.Where(x => x.ReferenceNumber.Contains(reference));
        if (!string.IsNullOrWhiteSpace(customer)) query = query.Where(x => (x.CustomerName != null && x.CustomerName.Contains(customer)) || (x.CustomerEmail != null && x.CustomerEmail.Contains(customer)) || (x.CustomerPhone != null && x.CustomerPhone.Contains(customer)));
        if (!string.IsNullOrWhiteSpace(keyword)) query = query.Where(x => x.ReferenceNumber.Contains(keyword) || x.ImageFileName.Contains(keyword) || (x.CustomerNotes != null && x.CustomerNotes.Contains(keyword)) || x.Material.NameEn.Contains(keyword));

        return await query.Select(x => new AdminServiceRequestListItemDto
        {
            Id = x.Id,
            RequestType = "laser",
            ReferenceNumber = x.ReferenceNumber,
            CustomerName = x.CustomerName,
            CustomerEmail = x.CustomerEmail,
            CustomerPhone = x.CustomerPhone,
            Title = x.ImageFileName,
            Summary = x.CustomerNotes,
            WorkflowStatus = ServiceRequestWorkflowMapper.FromLaser(x.Status).ToString(),
            LegacyStatus = x.Status.ToString(),
            CreatedAt = x.CreatedAt,
            OriginalModuleRoute = "/admin/laser-requests"
        }).ToListAsync();
    }

    private async Task<List<AdminServiceRequestListItemDto>> GetCncItems(DateTime? createdFrom, DateTime? createdTo, string? keyword, string? customer, string? reference)
    {
        var query = _context.Set<CncServiceRequest>().AsNoTracking().Include(x => x.Material).AsQueryable();
        query = ApplyDateFilter(query, createdFrom, createdTo);
        if (!string.IsNullOrWhiteSpace(reference)) query = query.Where(x => x.ReferenceNumber.Contains(reference));
        if (!string.IsNullOrWhiteSpace(customer)) query = query.Where(x => x.CustomerName.Contains(customer) || x.CustomerEmail.Contains(customer) || (x.CustomerPhone != null && x.CustomerPhone.Contains(customer)));
        if (!string.IsNullOrWhiteSpace(keyword)) query = query.Where(x => x.ReferenceNumber.Contains(keyword) || (x.FileName != null && x.FileName.Contains(keyword)) || (x.ProjectDescription != null && x.ProjectDescription.Contains(keyword)) || (x.DesignNotes != null && x.DesignNotes.Contains(keyword)));

        return await query.Select(x => new AdminServiceRequestListItemDto
        {
            Id = x.Id,
            RequestType = "cnc",
            ReferenceNumber = x.ReferenceNumber,
            CustomerName = x.CustomerName,
            CustomerEmail = x.CustomerEmail,
            CustomerPhone = x.CustomerPhone,
            Title = x.FileName ?? x.ServiceMode,
            Summary = x.ProjectDescription ?? x.DesignNotes,
            WorkflowStatus = ServiceRequestWorkflowMapper.FromCnc(x.Status).ToString(),
            LegacyStatus = x.Status.ToString(),
            CreatedAt = x.CreatedAt,
            OriginalModuleRoute = $"/admin/cnc-requests/{x.Id}"
        }).ToListAsync();
    }

    private async Task<AdminServiceRequestDetailDto?> GetProjectDetail(Guid id)
    {
        var entity = await _context.Set<ProjectRequest>().AsNoTracking().Include(x => x.Project).FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null) return null;

        var workflow = ServiceRequestWorkflowMapper.FromProject(entity.Status);
        return new AdminServiceRequestDetailDto
        {
            Id = entity.Id,
            RequestType = "project",
            RequestTypeName = entity.RequestType.ToString(),
            ReferenceNumber = entity.ReferenceNumber,
            CustomerName = entity.FullName,
            CustomerEmail = entity.Email,
            CustomerPhone = entity.Phone,
            Title = entity.Project?.TitleEn ?? "Project Request",
            Summary = entity.Description,
            Description = entity.Description,
            WorkflowStatus = workflow.ToString(),
            LegacyStatus = entity.Status.ToString(),
            CreatedAt = entity.CreatedAt,
            OriginalModuleRoute = "/admin/project-requests",
            AdminNotes = entity.AdminNotes,
            BudgetRange = entity.BudgetRange,
            Timeline = entity.Timeline,
            ProjectId = entity.ProjectId?.ToString(),
            ProjectTitle = entity.Project?.TitleEn,
            Category = entity.Category?.ToString(),
            ProjectType = entity.ProjectType,
            MainDomain = entity.MainDomain,
            ProjectStage = entity.ProjectStage,
            RequiredCapabilities = ProjectRequestSerialization.DeserializeCapabilities(entity),
            FileUrl = entity.AttachmentUrl,
            FileName = entity.AttachmentFileName,
            Attachments = ProjectRequestSerialization.DeserializeAttachments(entity)
                .Select(a => new AdminServiceRequestAttachmentDto
                {
                    Id = a.Url,
                    FileName = a.FileName,
                    Url = a.Url
                }).ToList(),
            History = BuildHistory(workflow, entity.CreatedAt, null, null, entity.UpdatedAt)
        };
    }

    private async Task<AdminServiceRequestDetailDto?> GetPrint3dDetail(Guid id)
    {
        var entity = await _context.Set<Print3dServiceRequest>().AsNoTracking()
            .Include(x => x.Material)
            .Include(x => x.MaterialColor)
            .Include(x => x.Profile)
            .Include(x => x.User)
            .Include(x => x.UsedSpool!).ThenInclude(x => x.Material)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null) return null;

        var workflow = ServiceRequestWorkflowMapper.FromPrint3d(entity.Status);
        return new AdminServiceRequestDetailDto
        {
            Id = entity.Id,
            RequestType = "print3d",
            ReferenceNumber = entity.ReferenceNumber,
            CustomerName = entity.CustomerName ?? entity.User?.FullName,
            CustomerEmail = entity.CustomerEmail ?? entity.User?.Email,
            CustomerPhone = entity.CustomerPhone,
            Title = entity.FileName,
            Summary = entity.CustomerNotes,
            WorkflowStatus = workflow.ToString(),
            LegacyStatus = entity.Status.ToString(),
            CreatedAt = entity.CreatedAt,
            OriginalModuleRoute = "/admin/3d-requests",
            AdminNotes = entity.AdminNotes,
            CustomerNotes = entity.CustomerNotes,
            FileName = entity.FileName,
            DownloadUrl = null,
            EstimatedPrice = entity.EstimatedPrice,
            FinalPrice = entity.FinalPrice,
            Currency = entity.Currency,
            ReviewedAt = entity.ReviewedAt,
            ApprovedAt = entity.ApprovedAt,
            CompletedAt = entity.CompletedAt,
            MaterialId = entity.MaterialId.ToString(),
            MaterialName = entity.Material?.NameEn,
            MaterialColorId = entity.MaterialColorId?.ToString(),
            MaterialColorName = entity.MaterialColor?.NameEn,
            ProfileId = entity.ProfileId?.ToString(),
            ProfileName = entity.Profile?.NameEn,
            EstimatedPrintTimeHours = entity.EstimatedPrintTimeHours,
            SuggestedPrice = entity.SuggestedPrice,
            EstimatedFilamentGrams = entity.EstimatedFilamentGrams,
            UsedSpoolId = entity.UsedSpoolId?.ToString(),
            UsedSpoolName = entity.UsedSpool == null ? null : $"{entity.UsedSpool.Material?.NameEn} - {entity.UsedSpool.Name}",
            History = BuildHistory(workflow, entity.CreatedAt, entity.ReviewedAt, entity.ApprovedAt, entity.CompletedAt)
        };
    }

    private async Task<AdminServiceRequestDetailDto?> GetDesignDetail(Guid id)
    {
        var entity = await _context.Set<DesignServiceRequest>().AsNoTracking().Include(x => x.User).Include(x => x.Attachments).FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null) return null;

        var workflow = ServiceRequestWorkflowMapper.FromDesign(entity.Status);
        return new AdminServiceRequestDetailDto
        {
            Id = entity.Id,
            RequestType = "design",
            RequestTypeName = entity.RequestType.ToString(),
            ReferenceNumber = entity.ReferenceNumber,
            CustomerName = entity.CustomerName ?? entity.User?.FullName,
            CustomerEmail = entity.CustomerEmail ?? entity.User?.Email,
            CustomerPhone = entity.CustomerPhone,
            Title = entity.Title,
            Summary = entity.Description,
            Description = entity.Description,
            WorkflowStatus = workflow.ToString(),
            LegacyStatus = entity.Status.ToString(),
            CreatedAt = entity.CreatedAt,
            OriginalModuleRoute = "/admin/design-requests",
            AdminNotes = entity.AdminNotes,
            CustomerNotes = null,
            QuotedPrice = entity.QuotedPrice,
            FinalPrice = entity.FinalPrice,
            BudgetRange = entity.BudgetRange,
            Timeline = entity.Timeline,
            DeliveryNotes = entity.DeliveryNotes,
            ReviewedAt = entity.ReviewedAt,
            CompletedAt = entity.DeliveredAt,
            IntendedUse = entity.IntendedUse,
            MaterialPreference = entity.MaterialPreference,
            DimensionsNotes = entity.DimensionsNotes,
            ToleranceLevel = entity.ToleranceLevel?.ToString(),
            IpOwnershipConfirmed = entity.IpOwnershipConfirmed,
            Attachments = entity.Attachments.Select(a => new AdminServiceRequestAttachmentDto
            {
                Id = a.Id.ToString(),
                FileName = a.FileName,
                Url = $"/api/v1/design-requests/{entity.Id}/attachments/{a.Id}",
                FileSizeBytes = a.FileSizeBytes,
                UploadedAt = a.UploadedAt
            }).ToList(),
            History = BuildHistory(workflow, entity.CreatedAt, entity.ReviewedAt, entity.QuotedAt, entity.DeliveredAt)
        };
    }

    private async Task<AdminServiceRequestDetailDto?> GetDesignCadDetail(Guid id)
    {
        var entity = await _context.Set<DesignCadServiceRequest>().AsNoTracking().Include(x => x.User).Include(x => x.Attachments).FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null) return null;

        var workflow = ServiceRequestWorkflowMapper.FromDesignCad(entity.Status);
        return new AdminServiceRequestDetailDto
        {
            Id = entity.Id,
            RequestType = "designCad",
            RequestTypeName = entity.RequestType.ToString(),
            ReferenceNumber = entity.ReferenceNumber,
            CustomerName = entity.CustomerName ?? entity.User?.FullName,
            CustomerEmail = entity.CustomerEmail ?? entity.User?.Email,
            CustomerPhone = entity.CustomerPhone,
            Title = entity.Title,
            Summary = entity.Description,
            Description = entity.Description,
            WorkflowStatus = workflow.ToString(),
            LegacyStatus = entity.Status.ToString(),
            CreatedAt = entity.CreatedAt,
            OriginalModuleRoute = "/admin/cad-requests",
            AdminNotes = entity.AdminNotes,
            CustomerNotes = entity.CustomerNotes,
            QuotedPrice = entity.QuotedPrice,
            FinalPrice = entity.FinalPrice,
            ReviewedAt = entity.ReviewedAt,
            CompletedAt = entity.CompletedAt,
            TargetProcess = entity.TargetProcess?.ToString(),
            IntendedUse = entity.IntendedUse,
            MaterialNotes = entity.MaterialNotes,
            DimensionsNotes = entity.DimensionsNotes,
            ToleranceNotes = entity.ToleranceNotes,
            WhatNeedsChange = entity.WhatNeedsChange,
            CriticalSurfaces = entity.CriticalSurfaces,
            FitmentRequirements = entity.FitmentRequirements,
            PurposeAndConstraints = entity.PurposeAndConstraints,
            Deadline = entity.Deadline,
            HasPhysicalPart = entity.HasPhysicalPart,
            LegalConfirmation = entity.LegalConfirmation,
            CanDeliverPhysicalPart = entity.CanDeliverPhysicalPart,
            Attachments = entity.Attachments.Select(a => new AdminServiceRequestAttachmentDto
            {
                Id = a.Id.ToString(),
                FileName = a.FileName,
                Url = $"/api/v1/design-cad-requests/{entity.Id}/attachments/{a.Id}",
                FileSizeBytes = a.FileSizeBytes,
                UploadedAt = a.UploadedAt
            }).ToList(),
            History = BuildHistory(workflow, entity.CreatedAt, entity.ReviewedAt, null, entity.CompletedAt)
        };
    }

    private async Task<AdminServiceRequestDetailDto?> GetLaserDetail(Guid id)
    {
        var entity = await _context.Set<LaserServiceRequest>().AsNoTracking().Include(x => x.Material).FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null) return null;

        var workflow = ServiceRequestWorkflowMapper.FromLaser(entity.Status);
        return new AdminServiceRequestDetailDto
        {
            Id = entity.Id,
            RequestType = "laser",
            ReferenceNumber = entity.ReferenceNumber,
            CustomerName = entity.CustomerName,
            CustomerEmail = entity.CustomerEmail,
            CustomerPhone = entity.CustomerPhone,
            Title = entity.ImageFileName,
            Summary = entity.CustomerNotes,
            WorkflowStatus = workflow.ToString(),
            LegacyStatus = entity.Status.ToString(),
            CreatedAt = entity.CreatedAt,
            OriginalModuleRoute = "/admin/laser-requests",
            AdminNotes = entity.AdminNotes,
            CustomerNotes = entity.CustomerNotes,
            QuotedPrice = entity.QuotedPrice,
            ReviewedAt = entity.ReviewedAt,
            CompletedAt = entity.CompletedAt,
            MaterialId = entity.MaterialId.ToString(),
            MaterialName = entity.Material.NameEn,
            OperationMode = entity.OperationMode,
            WidthCm = entity.WidthCm,
            HeightCm = entity.HeightCm,
            ImageUrl = $"/api/v1/laser/requests/{entity.Id}/image",
            History = BuildHistory(workflow, entity.CreatedAt, entity.ReviewedAt, null, entity.CompletedAt)
        };
    }

    private async Task<AdminServiceRequestDetailDto?> GetCncDetail(Guid id)
    {
        var entity = await _context.Set<CncServiceRequest>().AsNoTracking().Include(x => x.Material).FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null) return null;

        var workflow = ServiceRequestWorkflowMapper.FromCnc(entity.Status);
        return new AdminServiceRequestDetailDto
        {
            Id = entity.Id,
            RequestType = "cnc",
            ReferenceNumber = entity.ReferenceNumber,
            CustomerName = entity.CustomerName,
            CustomerEmail = entity.CustomerEmail,
            CustomerPhone = entity.CustomerPhone,
            Title = entity.FileName ?? entity.ServiceMode,
            Summary = entity.ProjectDescription ?? entity.DesignNotes,
            WorkflowStatus = workflow.ToString(),
            LegacyStatus = entity.Status.ToString(),
            CreatedAt = entity.CreatedAt,
            OriginalModuleRoute = $"/admin/cnc-requests/{entity.Id}",
            AdminNotes = entity.AdminNotes,
            EstimatedPrice = entity.EstimatedPrice,
            FinalPrice = entity.FinalPrice,
            ReviewedAt = entity.ReviewedAt,
            CompletedAt = entity.CompletedAt,
            MaterialId = entity.MaterialId?.ToString(),
            MaterialName = entity.Material?.NameEn,
            ServiceMode = entity.ServiceMode,
            OperationType = entity.OperationType,
            WidthMm = entity.WidthMm,
            HeightMm = entity.HeightMm,
            ThicknessMm = entity.ThicknessMm,
            Quantity = entity.Quantity,
            DepthMode = entity.DepthMode,
            DepthMm = entity.DepthMm,
            DesignSourceType = entity.DesignSourceType,
            DesignNotes = entity.DesignNotes,
            ProjectDescription = entity.ProjectDescription,
            PcbMaterial = entity.PcbMaterial,
            PcbThickness = entity.PcbThickness,
            PcbSide = entity.PcbSide,
            PcbOperation = entity.PcbOperation,
            FileName = entity.FileName,
            DownloadUrl = string.IsNullOrWhiteSpace(entity.FileName) ? null : $"/api/v1/cnc/requests/{entity.Id}/file",
            History = BuildHistory(workflow, entity.CreatedAt, entity.ReviewedAt, null, entity.CompletedAt)
        };
    }

    private static IQueryable<T> ApplyDateFilter<T>(IQueryable<T> query, DateTime? createdFrom, DateTime? createdTo) where T : class
    {
        if (createdFrom.HasValue) query = query.Where(BuildCreatedAtPredicate<T>(createdFrom.Value, true));
        if (createdTo.HasValue) query = query.Where(BuildCreatedAtPredicate<T>(createdTo.Value, false));
        return query;
    }

    private static System.Linq.Expressions.Expression<Func<T, bool>> BuildCreatedAtPredicate<T>(DateTime boundary, bool isFrom) where T : class
    {
        var parameter = System.Linq.Expressions.Expression.Parameter(typeof(T), "x");
        var createdAt = System.Linq.Expressions.Expression.Property(parameter, "CreatedAt");
        var constant = System.Linq.Expressions.Expression.Constant(boundary);
        var comparison = isFrom
            ? System.Linq.Expressions.Expression.GreaterThanOrEqual(createdAt, constant)
            : System.Linq.Expressions.Expression.LessThanOrEqual(createdAt, constant);
        return System.Linq.Expressions.Expression.Lambda<Func<T, bool>>(comparison, parameter);
    }

    private static string? NormalizeRequestType(string? raw) => raw?.Trim().ToLowerInvariant() switch
    {
        null or "" => null,
        "project" or "projects" => "project",
        "3d" or "3dprint" or "print3d" => "print3d",
        "design" => "design",
        "cad" or "designcad" => "designCad",
        "laser" => "laser",
        "cnc" => "cnc",
        _ => null
    };

    private static ServiceRequestWorkflowStatus? ParseWorkflowStatus(string? raw) =>
        string.IsNullOrWhiteSpace(raw)
            ? null
            : (Enum.TryParse<ServiceRequestWorkflowStatus>(raw, true, out var value) ? value : null);

    private static string MergeNotes(string? adminNotes, string? internalNote, string? existing)
    {
        var baseNotes = adminNotes ?? existing ?? string.Empty;
        if (string.IsNullOrWhiteSpace(internalNote)) return baseNotes;
        return string.IsNullOrWhiteSpace(baseNotes) ? internalNote.Trim() : $"{baseNotes}{Environment.NewLine}{internalNote.Trim()}";
    }

    private static string Coalesce(string? candidate, string existing) => string.IsNullOrWhiteSpace(candidate) ? existing : candidate.Trim();
    private static Guid ParseGuidOrKeep(string? value, Guid existing) => Guid.TryParse(value, out var parsed) ? parsed : existing;
    private static Guid? ParseNullableGuidOrKeep(string? value, Guid? existing) => string.IsNullOrWhiteSpace(value) ? existing : (Guid.TryParse(value, out var parsed) ? parsed : existing);

    private static List<AdminServiceRequestHistoryItemDto> BuildHistory(ServiceRequestWorkflowStatus currentStatus, DateTime createdAt, DateTime? reviewedAt, DateTime? approvalLikeAt, DateTime? completedAt)
    {
        var orderedStatuses = new[]
        {
            ServiceRequestWorkflowStatus.New,
            ServiceRequestWorkflowStatus.UnderReview,
            ServiceRequestWorkflowStatus.AwaitingCustomerConfirmation,
            ServiceRequestWorkflowStatus.Approved,
            ServiceRequestWorkflowStatus.InProgress,
            ServiceRequestWorkflowStatus.ReadyForDelivery,
            ServiceRequestWorkflowStatus.Completed
        };

        var currentIndex = Math.Max(Array.IndexOf(orderedStatuses, currentStatus), 0);
        return orderedStatuses.Select((status, index) => new AdminServiceRequestHistoryItemDto
        {
            Status = status.ToString(),
            Timestamp = status switch
            {
                ServiceRequestWorkflowStatus.New => createdAt,
                ServiceRequestWorkflowStatus.UnderReview => reviewedAt,
                ServiceRequestWorkflowStatus.AwaitingCustomerConfirmation => approvalLikeAt,
                ServiceRequestWorkflowStatus.Approved => approvalLikeAt,
                ServiceRequestWorkflowStatus.Completed => completedAt,
                _ => null
            },
            Reached = index <= currentIndex
        }).ToList();
    }
}
