using MediatR;
using Maba.Application.Features.AiChat.DTOs;

namespace Maba.Application.Features.AiChat.Sessions.Queries;

public class GetAllAiSessionsQuery : IRequest<List<AiSessionDto>>
{
    public Guid? UserId { get; set; }
    public Guid? AiSessionSourceId { get; set; }
}

