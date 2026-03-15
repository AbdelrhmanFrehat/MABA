using MediatR;
using Maba.Application.Features.AiChat.DTOs;

namespace Maba.Application.Features.AiChat.Sessions.Commands;

public class UpdateAiSessionCommand : IRequest<AiSessionDto>
{
    public Guid SessionId { get; set; }
    public string? Title { get; set; }
}

