using MediatR;
using Maba.Application.Features.AiChat.DTOs;

namespace Maba.Application.Features.AiChat.Messages.Commands;

public class CreateAiMessageCommand : IRequest<AiMessageDto>
{
    public Guid SessionId { get; set; }
    public Guid AiSenderTypeId { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? MetaJson { get; set; }
}

