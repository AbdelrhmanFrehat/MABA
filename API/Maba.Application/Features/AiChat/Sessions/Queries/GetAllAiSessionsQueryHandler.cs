using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.AiChat.DTOs;
using Maba.Application.Features.AiChat.Sessions.Queries;
using Maba.Domain.AiChat;

namespace Maba.Application.Features.AiChat.Sessions.Handlers;

public class GetAllAiSessionsQueryHandler : IRequestHandler<GetAllAiSessionsQuery, List<AiSessionDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllAiSessionsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AiSessionDto>> Handle(GetAllAiSessionsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Set<AiSession>()
            .Include(s => s.User)
            .Include(s => s.AiSessionSource)
            .Include(s => s.Messages)
            .ThenInclude(m => m.AiSenderType)
            .AsQueryable();

        if (request.UserId.HasValue)
        {
            query = query.Where(s => s.UserId == request.UserId.Value);
        }

        if (request.AiSessionSourceId.HasValue)
        {
            query = query.Where(s => s.AiSessionSourceId == request.AiSessionSourceId.Value);
        }

        var sessions = await query.ToListAsync(cancellationToken);

        return sessions.Select(s => new AiSessionDto
        {
            Id = s.Id,
            UserId = s.UserId,
            UserFullName = s.User?.FullName,
            AiSessionSourceId = s.AiSessionSourceId,
            AiSessionSourceKey = s.AiSessionSource?.Key,
            Title = s.Title,
            StartedAt = s.StartedAt,
            EndedAt = s.EndedAt,
            IsActive = s.IsActive,
            Messages = s.Messages.Select(m => new AiMessageDto
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
            }).ToList(),
            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt
        }).ToList();
    }
}

