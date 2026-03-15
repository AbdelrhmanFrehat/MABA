using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.AiChat.DTOs;
using System.Text.Json;

namespace Maba.Infrastructure.Services;

public class OpenAiSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gpt-4o-mini";
    public string EndpointUrl { get; set; } = "https://api.openai.com/v1/chat/completions";
}

public class OpenAiService : IAiService
{
    private readonly HttpClient _httpClient;
    private readonly OpenAiSettings _settings;
    private readonly ILogger<OpenAiService> _logger;
    private const string SystemMessage = "You are MABA's AI assistant and must answer clearly and concisely.";

    public OpenAiService(
        HttpClient httpClient,
        IOptions<OpenAiSettings> settings,
        ILogger<OpenAiService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.ApiKey}");
    }

    public async Task<AiResponseDto> SendMessageAsync(AiRequestDto request, CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            var messages = new List<OpenAiMessage>
            {
                new OpenAiMessage { Role = "system", Content = SystemMessage }
            };

            foreach (var historyMessage in request.History)
            {
                var role = historyMessage.AiSenderTypeKey.Equals("User", StringComparison.OrdinalIgnoreCase) ? "user" : "assistant";
                messages.Add(new OpenAiMessage { Role = role, Content = historyMessage.Text });
            }

            messages.Add(new OpenAiMessage { Role = "user", Content = request.UserMessage });

            var requestBody = new
            {
                model = _settings.Model,
                messages = messages,
                temperature = 0.7,
                max_tokens = 1000
            };

            var response = await _httpClient.PostAsJsonAsync(_settings.EndpointUrl, requestBody, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadFromJsonAsync<OpenAiResponse>(cancellationToken: cancellationToken);

            if (responseContent == null || responseContent.Choices == null || responseContent.Choices.Count == 0)
            {
                throw new InvalidOperationException("OpenAI API returned empty response");
            }

            var choice = responseContent.Choices[0];
            var usage = responseContent.Usage ?? new OpenAiUsage();

            var responseTimeMs = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;

            return new AiResponseDto
            {
                Reply = choice.Message?.Content ?? string.Empty,
                Model = responseContent.Model ?? _settings.Model,
                PromptTokens = usage.PromptTokens,
                CompletionTokens = usage.CompletionTokens,
                TotalTokens = usage.TotalTokens,
                ResponseTimeMs = responseTimeMs
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling OpenAI API for session {SessionId}", request.SessionId);
            
            var responseTimeMs = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;

            return new AiResponseDto
            {
                Reply = "Sorry, the AI service is temporarily unavailable. Please try again later.",
                Model = _settings.Model,
                PromptTokens = 0,
                CompletionTokens = 0,
                TotalTokens = 0,
                ResponseTimeMs = responseTimeMs
            };
        }
    }

    private class OpenAiMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
    }

    private class OpenAiResponse
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("model")]
        public string? Model { get; set; }

        [JsonPropertyName("choices")]
        public List<OpenAiChoice>? Choices { get; set; }

        [JsonPropertyName("usage")]
        public OpenAiUsage? Usage { get; set; }
    }

    private class OpenAiChoice
    {
        [JsonPropertyName("message")]
        public OpenAiMessage? Message { get; set; }

        [JsonPropertyName("finish_reason")]
        public string? FinishReason { get; set; }
    }

    private class OpenAiUsage
    {
        [JsonPropertyName("prompt_tokens")]
        public int PromptTokens { get; set; }

        [JsonPropertyName("completion_tokens")]
        public int CompletionTokens { get; set; }

        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; set; }
    }
}

