using System.Net.Http;
using System.Text.Json;
using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public class JobsService : IJobsService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ISettingsService _settingsService;

    public JobsService(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public async Task<IReadOnlyList<ControlCenterJobListItem>> GetJobsAsync(string? status = null, string? machineType = null, CancellationToken cancellationToken = default)
    {
        using var client = new HttpClient();
        var url = BuildUrl("/api/v1/control-center/jobs", status, machineType);
        using var response = await client.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonSerializer.DeserializeAsync<List<ControlCenterJobListItem>>(stream, JsonOptions, cancellationToken)
            ?? new List<ControlCenterJobListItem>();
    }

    public async Task<ControlCenterJobDetail?> GetJobAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var client = new HttpClient();
        var url = BuildUrl($"/api/v1/control-center/jobs/{id}");
        using var response = await client.GetAsync(url, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonSerializer.DeserializeAsync<ControlCenterJobDetail>(stream, JsonOptions, cancellationToken);
    }

    public async Task<ControlCenterJobDetail?> RunActionAsync(Guid id, string action, CancellationToken cancellationToken = default)
    {
        using var client = new HttpClient();
        var url = BuildUrl($"/api/v1/control-center/jobs/{id}/actions/{Uri.EscapeDataString(action)}");
        using var response = await client.PostAsync(url, content: null, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonSerializer.DeserializeAsync<ControlCenterJobDetail>(stream, JsonOptions, cancellationToken);
    }

    private string BuildUrl(string path, string? status = null, string? machineType = null)
    {
        var baseUrl = _settingsService.Load().ApiBaseUrl?.TrimEnd('/') ?? "https://api.mabasol.com";
        var builder = new UriBuilder($"{baseUrl}{path}");

        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(status))
        {
            query.Add($"status={Uri.EscapeDataString(status)}");
        }

        if (!string.IsNullOrWhiteSpace(machineType))
        {
            query.Add($"machineType={Uri.EscapeDataString(machineType)}");
        }

        builder.Query = string.Join("&", query);
        return builder.ToString();
    }
}
