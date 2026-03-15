using Maba.Application.Features.AiChat.DTOs;

namespace Maba.Application.Common.Interfaces;

public interface IAiService
{
    Task<AiResponseDto> SendMessageAsync(AiRequestDto request, CancellationToken cancellationToken = default);
}

