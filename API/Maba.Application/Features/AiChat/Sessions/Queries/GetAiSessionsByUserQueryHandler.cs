using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.AiChat.Sessions.Queries;
using Maba.Application.Features.AiChat.DTOs;
using Maba.Domain.AiChat;

namespace Maba.Application.Features.AiChat.Sessions.Handlers;

public class GetAiSessionsByUserQueryHandler : IRequestHandler<GetAiSessionsByUserQuery, List<AiSessionDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAiSessionsByUserQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AiSessionDto>> Handle(GetAiSessionsByUserQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Set<AiSession>()
            .Include(s => s.User)
            .Include(s => s.AiSessionSource)
            .Where(s => s.UserId == request.UserId);

        if (request.IsActive.HasValue)
        {
            query = query.Where(s => s.IsActive == request.IsActive.Value);
        }

        var sessions = await query
            .OrderByDescending(s => s.StartedAt)
            .ToListAsync(cancellationToken);

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
            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt
        }).ToList();
    }
}

