using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.AiChat.DTOs;
using Maba.Application.Features.AiChat.Messages.Queries;
using Maba.Domain.AiChat;

namespace Maba.Application.Features.AiChat.Messages.Handlers;

public class GetAllAiMessagesQueryHandler : IRequestHandler<GetAllAiMessagesQuery, List<AiMessageDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllAiMessagesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AiMessageDto>> Handle(GetAllAiMessagesQuery request, CancellationToken cancellationToken)
    {
        var messages = await _context.Set<AiMessage>()
            .Include(m => m.AiSenderType)
            .Where(m => m.SessionId == request.SessionId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(cancellationToken);

        return messages.Select(m => new AiMessageDto
        {
            Id = m.Id,
            SessionId = m.SessionId,
            AiSenderTypeId = m.AiSenderTypeId,
            AiSenderTypeKey = m.AiSenderType.Key,
            Text = m.Text,
            MetaJson = m.MetaJson,
            CreatedAt = m.CreatedAt,
            UpdatedAt = m.UpdatedAt
        }).ToList();
    }
}

