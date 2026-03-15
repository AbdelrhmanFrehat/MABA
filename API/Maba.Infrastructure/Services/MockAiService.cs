using Maba.Application.Common.Interfaces;
using Maba.Application.Features.AiChat.DTOs;

namespace Maba.Infrastructure.Services;

public class MockAiService : IAiService
{
    private readonly Random _random = new();

    public Task<AiResponseDto> SendMessageAsync(AiRequestDto request, CancellationToken cancellationToken = default)
    {
        var response = new AiResponseDto
        {
            Reply = $"This is a mock AI response to: {request.UserMessage}. In a real implementation, this would call an actual AI service like OpenAI, Azure OpenAI, or similar.",
            Model = "gpt-4-mock",
            PromptTokens = _random.Next(50, 150),
            CompletionTokens = _random.Next(30, 100),
            TotalTokens = _random.Next(80, 250),
            ResponseTimeMs = _random.Next(500, 2000)
        };

        return Task.FromResult(response);
    }
}

