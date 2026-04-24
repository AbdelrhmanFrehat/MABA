using System.Text.Json;
using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public class AppAnnouncementsService : IAppAnnouncementsService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ISettingsService _settingsService;
    private readonly IAuthSessionService _authSessionService;

    public AppAnnouncementsService(ISettingsService settingsService, IAuthSessionService authSessionService)
    {
        _settingsService = settingsService;
        _authSessionService = authSessionService;
    }

    public async Task<IReadOnlyList<HomeTickerItem>> GetActiveDesktopAnnouncementsAsync(CancellationToken cancellationToken = default)
    {
        if (!_authSessionService.IsAuthenticated)
            return Array.Empty<HomeTickerItem>();

        using var client = _authSessionService.CreateHttpClient();
        var baseUrl = _settingsService.Load().ApiBaseUrl?.TrimEnd('/') ?? "https://api.mabasol.com";
        using var response = await client.GetAsync($"{baseUrl}/api/v1/control-center/announcements", cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var items = await JsonSerializer.DeserializeAsync<List<AppAnnouncementDto>>(stream, JsonOptions, cancellationToken) ?? new List<AppAnnouncementDto>();

        return items
            .Select(item => new HomeTickerItem
            {
                Id = item.Id.ToString("N"),
                Message = item.Message,
                Type = string.IsNullOrWhiteSpace(item.Type) ? "Info" : item.Type.Trim(),
                ImageUri = NormalizeImageUrl(item.ImageUrl),
                DisplayOrder = item.DisplayOrder,
                IsActive = true,
                StartsAt = item.StartsAt.HasValue ? new DateTimeOffset(DateTime.SpecifyKind(item.StartsAt.Value, DateTimeKind.Utc)) : null,
                EndsAt = item.EndsAt.HasValue ? new DateTimeOffset(DateTime.SpecifyKind(item.EndsAt.Value, DateTimeKind.Utc)) : null,
                TargetPlatform = item.TargetPlatform
            })
            .OrderBy(item => item.DisplayOrder)
            .ThenBy(item => item.Message)
            .ToList();
    }

    private string? NormalizeImageUrl(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        var value = raw.Trim();
        if (value.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
            return value;

        if (Uri.TryCreate(value, UriKind.Absolute, out var absolute))
            return absolute.ToString();

        var baseUrl = _settingsService.Load().ApiBaseUrl?.TrimEnd('/') ?? "https://api.mabasol.com";
        return $"{baseUrl}/{value.TrimStart('/')}";
    }

    private sealed class AppAnnouncementDto
    {
        public Guid Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Type { get; set; }
        public int DisplayOrder { get; set; }
        public string TargetPlatform { get; set; } = "Desktop";
        public DateTime? StartsAt { get; set; }
        public DateTime? EndsAt { get; set; }
        public string? ImageUrl { get; set; }
    }
}
