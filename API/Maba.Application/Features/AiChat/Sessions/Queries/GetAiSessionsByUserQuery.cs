using MediatR;
using Maba.Application.Features.AiChat.DTOs;

namespace Maba.Application.Features.AiChat.Sessions.Queries;

public class GetAiSessionsByUserQuery : IRequest<List<AiSessionDto>>
{
    public Guid UserId { get; set; }
    public bool? IsActive { get; set; }
}

