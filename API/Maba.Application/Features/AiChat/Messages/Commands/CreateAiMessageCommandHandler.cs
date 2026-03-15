using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.AiChat.Messages.Commands;
using Maba.Application.Features.AiChat.DTOs;
using Maba.Domain.AiChat;

namespace Maba.Application.Features.AiChat.Messages.Handlers;

public class CreateAiMessageCommandHandler : IRequestHandler<CreateAiMessageCommand, AiMessageDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IAiService _aiService;
    private readonly ILogger<CreateAiMessageCommandHandler> _logger;

    public CreateAiMessageCommandHandler(
        IApplicationDbContext context,
        IAiService aiService,
        ILogger<CreateAiMessageCommandHandler> logger)
    {
        _context = context;
        _aiService = aiService;
        _logger = logger;
    }

    public async Task<AiMessageDto> Handle(CreateAiMessageCommand request, CancellationToken cancellationToken)
    {
        var session = await _context.Set<AiSession>()
            .FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken);

        if (session == null)
        {
            throw new KeyNotFoundException("AI session not found");
        }

        var userSenderType = await _context.Set<AiSenderType>()
            .FirstOrDefaultAsync(st => st.Id == request.AiSenderTypeId, cancellationToken);

        if (userSenderType == null)
        {
            throw new KeyNotFoundException("AI sender type not found");
        }

        var userMessage = new AiMessage
        {
            Id = Guid.NewGuid(),
            SessionId = request.SessionId,
            AiSenderTypeId = request.AiSenderTypeId,
            Text = request.Text,
            MetaJson = request.MetaJson
        };

        _context.Set<AiMessage>().Add(userMessage);
        await _context.SaveChangesAsync(cancellationToken);

        var previousMessages = await _context.Set<AiMessage>()
            .Include(m => m.AiSenderType)
            .Where(m => m.SessionId == request.SessionId && m.Id != userMessage.Id)
            .OrderByDescending(m => m.CreatedAt)
            .Take(20)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(cancellationToken);

        var history = previousMessages.Select(m => new AiMessageDto
        {
            Id = m.Id,
            SessionId = m.SessionId,
            AiSenderTypeId = m.AiSenderTypeId,
            AiSenderTypeKey = m.AiSenderType.Key,
            Text = m.Text,
            MetaJson = m.MetaJson,
            TokensUsed = m.TokensUsed,
            Model = m.Model,
            ResponseTimeMs = m.ResponseTimeMs,
            IsEdited = m.IsEdited,
            EditedAt = m.EditedAt,
            CreatedAt = m.CreatedAt,
            UpdatedAt = m.UpdatedAt
        }).ToList();

        history.Add(new AiMessageDto
        {
            Id = userMessage.Id,
            SessionId = userMessage.SessionId,
            AiSenderTypeId = userMessage.AiSenderTypeId,
            AiSenderTypeKey = userSenderType.Key,
            Text = userMessage.Text,
            MetaJson = userMessage.MetaJson,
            CreatedAt = userMessage.CreatedAt,
            UpdatedAt = userMessage.UpdatedAt
        });

        var aiSenderType = await _context.Set<AiSenderType>()
            .FirstOrDefaultAsync(st => st.Key == "AI", cancellationToken);

        if (aiSenderType == null)
        {
            throw new KeyNotFoundException("AI sender type 'AI' not found in database");
        }

        AiResponseDto aiResponse;
        try
        {
            var aiRequest = new AiRequestDto
            {
                SessionId = request.SessionId,
                UserMessage = request.Text,
                History = history
            };

            aiResponse = await _aiService.SendMessageAsync(aiRequest, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling AI service for session {SessionId}", request.SessionId);
            aiResponse = new AiResponseDto
            {
                Reply = "Sorry, the AI service is temporarily unavailable. Please try again later.",
                Model = "error",
                PromptTokens = 0,
                CompletionTokens = 0,
                TotalTokens = 0,
                ResponseTimeMs = 0
            };
        }

        var aiMessage = new AiMessage
        {
            Id = Guid.NewGuid(),
            SessionId = request.SessionId,
            AiSenderTypeId = aiSenderType.Id,
            Text = aiResponse.Reply,
            Model = aiResponse.Model,
            TokensUsed = aiResponse.TotalTokens,
            ResponseTimeMs = aiResponse.ResponseTimeMs,
            MetaJson = $"{{\"promptTokens\":{aiResponse.PromptTokens},\"completionTokens\":{aiResponse.CompletionTokens},\"totalTokens\":{aiResponse.TotalTokens}}}"
        };

        _context.Set<AiMessage>().Add(aiMessage);
        await _context.SaveChangesAsync(cancellationToken);

        return new AiMessageDto
        {
            Id = userMessage.Id,
            SessionId = userMessage.SessionId,
            AiSenderTypeId = userMessage.AiSenderTypeId,
            AiSenderTypeKey = userSenderType.Key,
            Text = userMessage.Text,
            MetaJson = userMessage.MetaJson,
            CreatedAt = userMessage.CreatedAt,
            UpdatedAt = userMessage.UpdatedAt
        };
    }
}

