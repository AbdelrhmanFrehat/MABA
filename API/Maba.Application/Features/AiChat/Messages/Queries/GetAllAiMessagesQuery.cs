using MediatR;
using Maba.Application.Features.AiChat.DTOs;

namespace Maba.Application.Features.AiChat.Messages.Queries;

public class GetAllAiMessagesQuery : IRequest<List<AiMessageDto>>
{
    public Guid SessionId { get; set; }
}

