using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Common.EmailTemplates.Queries;
using Maba.Application.Features.Common.EmailTemplates.DTOs;
using Maba.Domain.Common;

namespace Maba.Application.Features.Common.EmailTemplates.Handlers;

public class GetEmailTemplatesQueryHandler : IRequestHandler<GetEmailTemplatesQuery, List<EmailTemplateDto>>
{
    private readonly IApplicationDbContext _context;

    public GetEmailTemplatesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<EmailTemplateDto>> Handle(GetEmailTemplatesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Set<EmailTemplate>().AsQueryable();

        if (request.IsActive.HasValue)
        {
            query = query.Where(t => t.IsActive == request.IsActive.Value);
        }

        var templates = await query
            .OrderBy(t => t.Key)
            .ToListAsync(cancellationToken);

        return templates.Select(t => new EmailTemplateDto
        {
            Id = t.Id,
            Key = t.Key,
            SubjectEn = t.SubjectEn,
            SubjectAr = t.SubjectAr,
            BodyHtmlEn = t.BodyHtmlEn,
            BodyHtmlAr = t.BodyHtmlAr,
            BodyTextEn = t.BodyTextEn,
            BodyTextAr = t.BodyTextAr,
            Variables = t.Variables,
            IsActive = t.IsActive,
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt
        }).ToList();
    }
}

