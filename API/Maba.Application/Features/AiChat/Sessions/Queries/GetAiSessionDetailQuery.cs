using MediatR;
using Maba.Application.Features.AiChat.DTOs;

namespace Maba.Application.Features.AiChat.Sessions.Queries;

public class GetAiSessionDetailQuery : IRequest<AiSessionDetailDto>
{
    public Guid SessionId { get; set; }
}

