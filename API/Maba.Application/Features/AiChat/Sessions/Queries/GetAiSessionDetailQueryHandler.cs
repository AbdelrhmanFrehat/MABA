using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.AiChat.Sessions.Queries;
using Maba.Application.Features.AiChat.DTOs;
using Maba.Domain.AiChat;

namespace Maba.Application.Features.AiChat.Sessions.Handlers;

public class GetAiSessionDetailQueryHandler : IRequestHandler<GetAiSessionDetailQuery, AiSessionDetailDto>
{
    private readonly IApplicationDbContext _context;

    public GetAiSessionDetailQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AiSessionDetailDto> Handle(GetAiSessionDetailQuery request, CancellationToken cancellationToken)
    {
        var session = await _context.Set<AiSession>()
            .Include(s => s.User)
            .Include(s => s.AiSessionSource)
            .Include(s => s.Messages)
            .ThenInclude(m => m.AiSenderType)
            .FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken);

        if (session == null)
        {
            throw new KeyNotFoundException("AI session not found.");
        }

        var totalTokensUsed = session.Messages
            .Where(m => m.TokensUsed.HasValue)
            .Sum(m => m.TokensUsed.Value);

        // Simple cost estimation (can be enhanced with actual pricing)
        var estimatedCost = totalTokensUsed > 0 
            ? (decimal?)(totalTokensUsed * 0.000002m) // Example: $0.002 per 1K tokens
            : null;

        return new AiSessionDetailDto
        {
            Id = session.Id,
            UserId = session.UserId,
            UserFullName = session.User?.FullName,
            AiSessionSourceId = session.AiSessionSourceId,
            AiSessionSourceKey = session.AiSessionSource?.Key,
            Title = session.Title,
            StartedAt = session.StartedAt,
            EndedAt = session.EndedAt,
            IsActive = session.IsActive,
            Messages = session.Messages
                .OrderBy(m => m.CreatedAt)
                .Select(m => new AiMessageDto
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
                })
                .ToList(),
            MessagesCount = session.Messages.Count,
            TotalTokensUsed = totalTokensUsed,
            EstimatedCost = estimatedCost,
            CreatedAt = session.CreatedAt,
            UpdatedAt = session.UpdatedAt
        };
    }
}

