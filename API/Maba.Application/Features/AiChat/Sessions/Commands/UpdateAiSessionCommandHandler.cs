using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.AiChat.Sessions.Commands;
using Maba.Application.Features.AiChat.DTOs;
using Maba.Domain.AiChat;

namespace Maba.Application.Features.AiChat.Sessions.Handlers;

public class UpdateAiSessionCommandHandler : IRequestHandler<UpdateAiSessionCommand, AiSessionDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateAiSessionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AiSessionDto> Handle(UpdateAiSessionCommand request, CancellationToken cancellationToken)
    {
        var session = await _context.Set<AiSession>()
            .Include(s => s.User)
            .Include(s => s.AiSessionSource)
            .FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken);

        if (session == null)
        {
            throw new KeyNotFoundException("AI session not found.");
        }

        if (request.Title != null)
        {
            session.Title = request.Title;
        }

        session.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new AiSessionDto
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
            CreatedAt = session.CreatedAt,
            UpdatedAt = session.UpdatedAt
        };
    }
}

