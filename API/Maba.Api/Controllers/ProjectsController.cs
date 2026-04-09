using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Maba.Application.Features.Projects.Commands;
using Maba.Application.Features.Projects.DTOs;
using Maba.Application.Features.Projects.Queries;
using Maba.Domain.Projects;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProjectsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<ProjectsListResponse>> GetProjects(
        [FromQuery] string? search,
        [FromQuery] ProjectCategory? category,
        [FromQuery] ProjectStatus? status,
        [FromQuery] bool? isFeatured,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12)
    {
        var result = await _mediator.Send(new GetProjectsQuery
        {
            SearchTerm = search,
            Category = category,
            Status = status,
            IsFeatured = isFeatured,
            IsActive = true,
            Page = page,
            PageSize = pageSize
        });

        return Ok(result);
    }

    [HttpGet("{slug}")]
    public async Task<ActionResult<ProjectDto>> GetProjectBySlug(string slug)
    {
        var result = await _mediator.Send(new GetProjectBySlugQuery { Slug = slug });
        
        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpGet("categories")]
    public ActionResult<IEnumerable<object>> GetCategories()
    {
        var categories = Enum.GetValues<ProjectCategory>()
            .Select(c => new { value = (int)c, name = c.ToString() });
        return Ok(categories);
    }

    [HttpGet("statuses")]
    public ActionResult<IEnumerable<object>> GetStatuses()
    {
        var statuses = Enum.GetValues<ProjectStatus>()
            .Where(s => s == ProjectStatus.Concept || s == ProjectStatus.Prototype || s == ProjectStatus.Delivered)
            .Select(s => new { value = (int)s, name = s.ToString() });
        return Ok(statuses);
    }

    [HttpPost("request")]
    public async Task<ActionResult<ProjectRequestDto>> SubmitProjectRequest([FromBody] CreateProjectRequestRfq request)
    {
        try
        {
            // Extract optional userId from JWT (populated even on anonymous endpoints when token is present)
            Guid? userId = null;
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var parsedUserId))
            {
                userId = parsedUserId;
            }

            var result = await _mediator.Send(new CreateProjectRequestCommand
            {
                FullName = request.FullName,
                Email = request.Email,
                Phone = request.Phone,
                RequestType = request.RequestType,
                ProjectId = request.ProjectId,
                ProjectType = request.ProjectType,
                MainDomain = request.MainDomain,
                RequiredCapabilities = request.RequiredCapabilities,
                Category = request.Category,
                BudgetRange = request.BudgetRange,
                Timeline = request.Timeline,
                ProjectStage = request.ProjectStage,
                Description = string.IsNullOrWhiteSpace(request.ProjectDescription) ? request.Description : request.ProjectDescription,
                AttachmentUrl = request.AttachmentUrl,
                AttachmentFileName = request.AttachmentFileName,
                Attachments = request.Attachments,
                UserId = userId
            });

            return Ok(new
            {
                success = true,
                referenceNumber = result.ReferenceNumber,
                message = "Your project request has been submitted successfully."
            });
        }
        catch (Exception ex)
        {
            var message = ex.InnerException?.Message ?? ex.Message;
            return BadRequest(new { success = false, message });
        }
    }

    [HttpPost("request/upload")]
    public async Task<ActionResult> UploadAttachment(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "No file provided" });
        }

        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "project-requests");
        Directory.CreateDirectory(uploadsFolder);

        var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return Ok(new
        {
            url = $"/uploads/project-requests/{uniqueFileName}",
            fileName = file.FileName
        });
    }

    // Admin endpoints
    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProjectsListResponse>> GetAllProjectsAdmin(
        [FromQuery] string? search,
        [FromQuery] ProjectCategory? category,
        [FromQuery] ProjectStatus? status,
        [FromQuery] bool? isFeatured,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetProjectsQuery
        {
            SearchTerm = search,
            Category = category,
            Status = status,
            IsFeatured = isFeatured,
            IsActive = isActive,
            Page = page,
            PageSize = pageSize
        });

        return Ok(result);
    }

    [HttpGet("admin/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProjectDto>> GetProjectById(Guid id)
    {
        var result = await _mediator.Send(new GetProjectByIdQuery { Id = id });
        
        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpPost("admin")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProjectDto>> CreateProject([FromBody] CreateProjectRequest request)
    {
        var result = await _mediator.Send(new CreateProjectCommand
        {
            TitleEn = request.TitleEn,
            TitleAr = request.TitleAr,
            Slug = request.Slug,
            SummaryEn = request.SummaryEn,
            SummaryAr = request.SummaryAr,
            DescriptionEn = request.DescriptionEn,
            DescriptionAr = request.DescriptionAr,
            CoverImageUrl = request.CoverImageUrl,
            Category = request.Category,
            Status = request.Status,
            Year = request.Year,
            TechStack = request.TechStack,
            Highlights = request.Highlights,
            Gallery = request.Gallery,
            IsFeatured = request.IsFeatured,
            IsActive = request.IsActive,
            SortOrder = request.SortOrder
        });

        return Ok(result);
    }

    [HttpPut("admin/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> UpdateProject(Guid id, [FromBody] UpdateProjectRequest request)
    {
        if (id != request.Id)
        {
            return BadRequest(new { message = "ID mismatch" });
        }

        var success = await _mediator.Send(new UpdateProjectCommand
        {
            Id = id,
            TitleEn = request.TitleEn,
            TitleAr = request.TitleAr,
            Slug = request.Slug,
            SummaryEn = request.SummaryEn,
            SummaryAr = request.SummaryAr,
            DescriptionEn = request.DescriptionEn,
            DescriptionAr = request.DescriptionAr,
            CoverImageUrl = request.CoverImageUrl,
            Category = request.Category,
            Status = request.Status,
            Year = request.Year,
            TechStack = request.TechStack,
            Highlights = request.Highlights,
            Gallery = request.Gallery,
            IsFeatured = request.IsFeatured,
            IsActive = request.IsActive,
            SortOrder = request.SortOrder
        });

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("admin/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteProject(Guid id)
    {
        var success = await _mediator.Send(new DeleteProjectCommand { Id = id });

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    // Project Requests Admin
    [HttpGet("admin/requests")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<ProjectRequestDto>>> GetProjectRequests(
        [FromQuery] ProjectRequestStatus? status,
        [FromQuery] string? workflowStatus,
        [FromQuery] ProjectRequestType? requestType,
        [FromQuery] string? projectType,
        [FromQuery] string? mainDomain,
        [FromQuery] string? projectStage,
        [FromQuery] string? search,
        [FromQuery] int? skip,
        [FromQuery] int? take)
    {
        var result = await _mediator.Send(new GetProjectRequestsQuery
        {
            Status = status,
            WorkflowStatus = workflowStatus,
            RequestType = requestType,
            ProjectType = projectType,
            MainDomain = mainDomain,
            ProjectStage = projectStage,
            SearchTerm = search,
            Skip = skip,
            Take = take
        });

        return Ok(result);
    }

    [HttpPut("admin/requests/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> UpdateProjectRequest(Guid id, [FromBody] UpdateProjectRequestRfq request)
    {
        if (id != request.Id)
            return BadRequest(new { message = "ID mismatch" });

        try
        {
            var success = await _mediator.Send(new UpdateProjectRequestCommand
            {
                Id = id,
                Status = request.Status,
                WorkflowStatus = request.WorkflowStatus,
                AdminNotes = request.AdminNotes,
                FullName = request.FullName,
                Email = request.Email,
                Phone = request.Phone,
                RequestType = request.RequestType,
                ProjectId = request.ProjectId,
                ProjectType = request.ProjectType,
                MainDomain = request.MainDomain,
                RequiredCapabilities = request.RequiredCapabilities,
                Category = request.Category,
                BudgetRange = request.BudgetRange,
                Timeline = request.Timeline,
                ProjectStage = request.ProjectStage,
                Description = string.IsNullOrWhiteSpace(request.ProjectDescription) ? request.Description : request.ProjectDescription,
                AttachmentUrl = request.AttachmentUrl,
                AttachmentFileName = request.AttachmentFileName,
                Attachments = request.Attachments,
                AssignedToUserId = request.AssignedToUserId,
                AssignedToName = request.AssignedToName,
                Priority = request.Priority,
                TechnicalFeasibility = request.TechnicalFeasibility,
                EstimatedCost = request.EstimatedCost,
                EstimatedTimeline = request.EstimatedTimeline,
                ComplexityLevel = request.ComplexityLevel,
                InternalNotes = request.InternalNotes,
                UpdatedBy = request.UpdatedBy
            });

            if (!success) return NotFound();
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpGet("admin/requests/{id:guid}/activities")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<ProjectRequestActivityDto>>> GetProjectRequestActivities(Guid id)
    {
        var result = await _mediator.Send(new GetProjectRequestActivitiesQuery { ProjectRequestId = id });
        return Ok(result);
    }
}
