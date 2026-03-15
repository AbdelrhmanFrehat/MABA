using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.AiChat.Sessions.Commands;
using Maba.Application.Features.AiChat.DTOs;
using Maba.Domain.AiChat;

namespace Maba.Application.Features.AiChat.Sessions.Handlers;

public class CreateAiSessionCommandHandler : IRequestHandler<CreateAiSessionCommand, AiSessionDto>
{
    private readonly IApplicationDbContext _context;

    public CreateAiSessionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AiSessionDto> Handle(CreateAiSessionCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId.HasValue)
        {
            var userExists = await _context.Set<Domain.Users.User>()
                .AnyAsync(u => u.Id == request.UserId.Value, cancellationToken);

            if (!userExists)
            {
                throw new KeyNotFoundException("User not found");
            }
        }

        if (request.AiSessionSourceId.HasValue)
        {
            var sourceExists = await _context.Set<AiSessionSource>()
                .AnyAsync(s => s.Id == request.AiSessionSourceId.Value, cancellationToken);

            if (!sourceExists)
            {
                throw new KeyNotFoundException("AI session source not found");
            }
        }

        var session = new AiSession
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            AiSessionSourceId = request.AiSessionSourceId,
            StartedAt = DateTime.UtcNow
        };

        _context.Set<AiSession>().Add(session);
        await _context.SaveChangesAsync(cancellationToken);

        // Load session with relations
        var sessionWithRelations = await _context.Set<AiSession>()
            .Include(s => s.User)
            .Include(s => s.AiSessionSource)
            .FirstOrDefaultAsync(s => s.Id == session.Id, cancellationToken);

        return new AiSessionDto
        {
            Id = sessionWithRelations!.Id,
            UserId = sessionWithRelations.UserId,
            UserFullName = sessionWithRelations.User?.FullName,
            AiSessionSourceId = sessionWithRelations.AiSessionSourceId,
            AiSessionSourceKey = sessionWithRelations.AiSessionSource?.Key,
            StartedAt = sessionWithRelations.StartedAt,
            Messages = new List<AiMessageDto>(),
            CreatedAt = sessionWithRelations.CreatedAt,
            UpdatedAt = sessionWithRelations.UpdatedAt
        };
    }
}

