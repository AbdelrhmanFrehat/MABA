using MediatR;
using Maba.Application.Features.AiChat.DTOs;

namespace Maba.Application.Features.AiChat.Sessions.Commands;

public class CreateAiSessionCommand : IRequest<AiSessionDto>
{
    public Guid? UserId { get; set; }
    public Guid? AiSessionSourceId { get; set; }
}

