using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Common.EmailTemplates.Commands;
using Maba.Application.Features.Common.EmailTemplates.DTOs;
using Maba.Domain.Common;

namespace Maba.Application.Features.Common.EmailTemplates.Handlers;

public class UpdateEmailTemplateCommandHandler : IRequestHandler<UpdateEmailTemplateCommand, EmailTemplateDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateEmailTemplateCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EmailTemplateDto> Handle(UpdateEmailTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await _context.Set<EmailTemplate>()
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (template == null)
        {
            throw new KeyNotFoundException("Email template not found.");
        }

        if (request.SubjectEn != null)
        {
            template.SubjectEn = request.SubjectEn;
        }

        if (request.SubjectAr != null)
        {
            template.SubjectAr = request.SubjectAr;
        }

        if (request.BodyHtmlEn != null)
        {
            template.BodyHtmlEn = request.BodyHtmlEn;
        }

        if (request.BodyHtmlAr != null)
        {
            template.BodyHtmlAr = request.BodyHtmlAr;
        }

        if (request.BodyTextEn != null)
        {
            template.BodyTextEn = request.BodyTextEn;
        }

        if (request.BodyTextAr != null)
        {
            template.BodyTextAr = request.BodyTextAr;
        }

        if (request.Variables != null)
        {
            template.Variables = request.Variables;
        }

        if (request.IsActive.HasValue)
        {
            template.IsActive = request.IsActive.Value;
        }

        template.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new EmailTemplateDto
        {
            Id = template.Id,
            Key = template.Key,
            SubjectEn = template.SubjectEn,
            SubjectAr = template.SubjectAr,
            BodyHtmlEn = template.BodyHtmlEn,
            BodyHtmlAr = template.BodyHtmlAr,
            BodyTextEn = template.BodyTextEn,
            BodyTextAr = template.BodyTextAr,
            Variables = template.Variables,
            IsActive = template.IsActive,
            CreatedAt = template.CreatedAt,
            UpdatedAt = template.UpdatedAt
        };
    }
}

