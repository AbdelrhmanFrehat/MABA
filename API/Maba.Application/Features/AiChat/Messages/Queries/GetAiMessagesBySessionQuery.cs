using MediatR;
using Maba.Application.Features.AiChat.DTOs;

namespace Maba.Application.Features.AiChat.Messages.Queries;

public class GetAiMessagesBySessionQuery : IRequest<List<AiMessageDto>>
{
    public Guid SessionId { get; set; }
    public int? Limit { get; set; } // Optional limit for pagination
}

