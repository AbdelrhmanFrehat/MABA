using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.AiChat.Messages.Queries;
using Maba.Application.Features.AiChat.DTOs;
using Maba.Domain.AiChat;

namespace Maba.Application.Features.AiChat.Messages.Handlers;

public class GetAiMessagesBySessionQueryHandler : IRequestHandler<GetAiMessagesBySessionQuery, List<AiMessageDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAiMessagesBySessionQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AiMessageDto>> Handle(GetAiMessagesBySessionQuery request, CancellationToken cancellationToken)
    {
        var session = await _context.Set<AiSession>()
            .FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken);

        if (session == null)
        {
            throw new KeyNotFoundException("AI session not found.");
        }

        var query = _context.Set<AiMessage>()
            .Include(m => m.AiSenderType)
            .Where(m => m.SessionId == request.SessionId);

        if (request.Limit.HasValue && request.Limit.Value > 0)
        {
            query = query.OrderByDescending(m => m.CreatedAt).Take(request.Limit.Value);
        }
        else
        {
            query = query.OrderBy(m => m.CreatedAt);
        }

        var messages = await query.ToListAsync(cancellationToken);

        return messages.Select(m => new AiMessageDto
        {
            Id = m.Id,
            SessionId = m.SessionId,
            AiSenderTypeId = m.AiSenderTypeId,
            AiSenderTypeKey = m.AiSenderType.Key,
            Text = m.Text,
            MetaJson = m.MetaJson,
            TokensUsed = m.TokensUsed,
            Model = m.Model,
            ResponseTimeMs = m.ResponseTimeMs,
            IsEdited = m.IsEdited,
            EditedAt = m.EditedAt,
            CreatedAt = m.CreatedAt,
            UpdatedAt = m.UpdatedAt
        }).ToList();
    }
}

