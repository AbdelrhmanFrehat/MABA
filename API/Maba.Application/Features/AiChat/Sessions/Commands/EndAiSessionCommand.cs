using MediatR;
using Maba.Application.Features.AiChat.DTOs;

namespace Maba.Application.Features.AiChat.Sessions.Commands;

public class EndAiSessionCommand : IRequest<AiSessionDto>
{
    public Guid SessionId { get; set; }
}

