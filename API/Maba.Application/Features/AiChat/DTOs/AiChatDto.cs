namespace Maba.Application.Features.AiChat.DTOs;

public class AiSessionDto
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string? UserFullName { get; set; }
    public Guid? AiSessionSourceId { get; set; }
    public string? AiSessionSourceKey { get; set; }
    public string? Title { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public bool IsActive { get; set; }
    public List<AiMessageDto> Messages { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class AiSessionDetailDto : AiSessionDto
{
    public int MessagesCount { get; set; }
    public int TotalTokensUsed { get; set; }
    public decimal? EstimatedCost { get; set; }
}

public class AiMessageDto
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public Guid AiSenderTypeId { get; set; }
    public string AiSenderTypeKey { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string? MetaJson { get; set; }
    public int? TokensUsed { get; set; }
    public string? Model { get; set; }
    public int? ResponseTimeMs { get; set; }
    public bool IsEdited { get; set; }
    public DateTime? EditedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

