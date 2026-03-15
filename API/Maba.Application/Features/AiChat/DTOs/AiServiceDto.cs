namespace Maba.Application.Features.AiChat.DTOs;

public class AiRequestDto
{
    public Guid SessionId { get; set; }
    public string UserMessage { get; set; } = string.Empty;
    public List<AiMessageDto> History { get; set; } = new();
}

public class AiResponseDto
{
    public string Reply { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int PromptTokens { get; set; }
    public int CompletionTokens { get; set; }
    public int TotalTokens { get; set; }
    public int ResponseTimeMs { get; set; }
}

