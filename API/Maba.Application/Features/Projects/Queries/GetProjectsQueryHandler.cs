using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Projects.DTOs;
using Maba.Domain.Projects;

namespace Maba.Application.Features.Projects.Queries;

public class GetProjectsQueryHandler : IRequestHandler<GetProjectsQuery, ProjectsListResponse>
{
    private readonly IApplicationDbContext _context;

    public GetProjectsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProjectsListResponse> Handle(GetProjectsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Set<Project>().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(p => 
                p.TitleEn.ToLower().Contains(term) ||
                (p.TitleAr != null && p.TitleAr.ToLower().Contains(term)) ||
                (p.SummaryEn != null && p.SummaryEn.ToLower().Contains(term)));
        }

        if (request.Category.HasValue)
        {
            query = query.Where(p => p.Category == request.Category.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(p => p.Status == request.Status.Value);
        }

        if (request.IsFeatured.HasValue)
        {
            query = query.Where(p => p.IsFeatured == request.IsFeatured.Value);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(p => p.IsActive == request.IsActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var projects = await query
            .OrderByDescending(p => p.IsFeatured)
            .ThenBy(p => p.SortOrder)
            .ThenByDescending(p => p.Year)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var items = projects.Select(p => new ProjectListDto
        {
            Id = p.Id,
            TitleEn = p.TitleEn,
            TitleAr = p.TitleAr,
            Slug = p.Slug,
            SummaryEn = p.SummaryEn,
            SummaryAr = p.SummaryAr,
            CoverImageUrl = p.CoverImageUrl,
            Category = p.Category,
            Status = p.Status,
            Year = p.Year,
            TechStack = string.IsNullOrEmpty(p.TechStackJson) 
                ? new List<string>() 
                : JsonSerializer.Deserialize<List<string>>(p.TechStackJson) ?? new List<string>(),
            IsFeatured = p.IsFeatured
        }).ToList();

        return new ProjectsListResponse
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
