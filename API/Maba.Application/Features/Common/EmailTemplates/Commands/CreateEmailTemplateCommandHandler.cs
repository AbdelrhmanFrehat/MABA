using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Common.EmailTemplates.Commands;
using Maba.Application.Features.Common.EmailTemplates.DTOs;
using Maba.Domain.Common;

namespace Maba.Application.Features.Common.EmailTemplates.Handlers;

public class CreateEmailTemplateCommandHandler : IRequestHandler<CreateEmailTemplateCommand, EmailTemplateDto>
{
    private readonly IApplicationDbContext _context;

    public CreateEmailTemplateCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EmailTemplateDto> Handle(CreateEmailTemplateCommand request, CancellationToken cancellationToken)
    {
        // Check if key already exists
        var existing = await _context.Set<EmailTemplate>()
            .FirstOrDefaultAsync(t => t.Key == request.Key, cancellationToken);

        if (existing != null)
        {
            throw new InvalidOperationException($"Email template with key '{request.Key}' already exists.");
        }

        var template = new EmailTemplate
        {
            Id = Guid.NewGuid(),
            Key = request.Key,
            SubjectEn = request.SubjectEn,
            SubjectAr = request.SubjectAr,
            BodyHtmlEn = request.BodyHtmlEn,
            BodyHtmlAr = request.BodyHtmlAr,
            BodyTextEn = request.BodyTextEn,
            BodyTextAr = request.BodyTextAr,
            Variables = request.Variables,
            IsActive = request.IsActive
        };

        _context.Set<EmailTemplate>().Add(template);
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

