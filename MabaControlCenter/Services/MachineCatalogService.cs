using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public class MachineCatalogService : IMachineCatalogService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private readonly ISettingsService _settingsService;
    private readonly string _cacheFilePath;
    private MachineDefinitionCacheStore _store = new();

    public MachineCatalogService(ISettingsService settingsService)
    {
        _settingsService = settingsService;
        var baseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MabaControlCenter");
        _cacheFilePath = Path.Combine(baseDir, "machine-definitions-cache.json");
        Categories = new ObservableCollection<MachineCategory>();
        Families = new ObservableCollection<MachineFamily>();
        DefinitionSummaries = new ObservableCollection<MachineDefinitionSummary>();
        CachedDefinitions = new ObservableCollection<MachineDefinition>();
        LoadCache();
    }

    public ObservableCollection<MachineCategory> Categories { get; }
    public ObservableCollection<MachineFamily> Families { get; }
    public ObservableCollection<MachineDefinitionSummary> DefinitionSummaries { get; }
    public ObservableCollection<MachineDefinition> CachedDefinitions { get; }
    public string LastSyncStatus => _store.LastSyncStatus;
    public DateTime? LastSyncedAt => _store.LastSyncedAt == default ? null : _store.LastSyncedAt;

    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = new HttpClient();
            var categories = await GetAsync<List<MachineCategory>>(client, "/api/v1/machine-categories?isActive=true", cancellationToken) ?? new();
            var families = await GetAsync<List<MachineFamily>>(client, "/api/v1/machine-families?isActive=true", cancellationToken) ?? new();
            var summaries = await GetAsync<List<MachineDefinitionSummary>>(client, "/api/v1/machine-definitions?activeOnly=true&includeDeprecated=false", cancellationToken) ?? new();

            var definitions = new List<MachineDefinition>();
            foreach (var summary in summaries)
            {
                var definition = await GetAsync<MachineDefinition>(client, $"/api/v1/machine-definitions/{summary.Id}", cancellationToken);
                if (definition != null)
                    definitions.Add(definition);
            }

            _store = new MachineDefinitionCacheStore
            {
                Categories = categories,
                Families = families,
                DefinitionSummaries = summaries,
                Definitions = definitions,
                LastSyncedAt = DateTime.UtcNow,
                LastSyncStatus = $"Synced {definitions.Count} machine definition(s)."
            };
            SaveCache();
            PopulateCollections();
        }
        catch (Exception ex)
        {
            _store.LastSyncStatus = CachedDefinitions.Count > 0
                ? $"Backend unavailable. Using cached machine definitions. {ex.Message}"
                : $"Backend unavailable and no machine definition cache exists. {ex.Message}";
            SaveCache();
            PopulateCollections();
        }
    }

    public MachineDefinition? GetCachedDefinition(Guid definitionId, string? version = null)
    {
        return CachedDefinitions
            .Where(d => d.Id == definitionId)
            .OrderByDescending(d => string.Equals(d.Version, version, StringComparison.OrdinalIgnoreCase))
            .ThenByDescending(d => d.UpdatedAt ?? d.CreatedAt)
            .FirstOrDefault();
    }

    public async Task<MachineDefinition?> GetDefinitionAsync(Guid definitionId, CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = new HttpClient();
            var definition = await GetAsync<MachineDefinition>(client, $"/api/v1/machine-definitions/{definitionId}", cancellationToken);
            if (definition == null)
                return GetCachedDefinition(definitionId);

            var existing = _store.Definitions.FirstOrDefault(d => d.Id == definition.Id && d.Version == definition.Version);
            if (existing != null)
                _store.Definitions.Remove(existing);
            _store.Definitions.Add(definition);
            SaveCache();
            PopulateCollections();
            return definition;
        }
        catch
        {
            return GetCachedDefinition(definitionId);
        }
    }

    private async Task<T?> GetAsync<T>(HttpClient client, string path, CancellationToken cancellationToken)
    {
        var baseUrl = _settingsService.Load().ApiBaseUrl?.TrimEnd('/') ?? "https://api.mabasol.com";
        using var response = await client.GetAsync($"{baseUrl}{path}", cancellationToken);
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions, cancellationToken);
    }

    private void LoadCache()
    {
        try
        {
            if (File.Exists(_cacheFilePath))
            {
                var json = File.ReadAllText(_cacheFilePath);
                _store = JsonSerializer.Deserialize<MachineDefinitionCacheStore>(json, JsonOptions) ?? new MachineDefinitionCacheStore();
            }
        }
        catch
        {
            _store = new MachineDefinitionCacheStore { LastSyncStatus = "Machine definition cache could not be read." };
        }

        PopulateCollections();
    }

    private void SaveCache()
    {
        try
        {
            var dir = Path.GetDirectoryName(_cacheFilePath);
            if (!string.IsNullOrWhiteSpace(dir))
                Directory.CreateDirectory(dir);
            File.WriteAllText(_cacheFilePath, JsonSerializer.Serialize(_store, JsonOptions));
        }
        catch
        {
            // Cache persistence should not block runtime operation.
        }
    }

    private void PopulateCollections()
    {
        Replace(Categories, _store.Categories.OrderBy(c => c.SortOrder).ThenBy(c => c.DisplayNameEn));
        Replace(Families, _store.Families.OrderBy(f => f.SortOrder).ThenBy(f => f.DisplayNameEn));
        Replace(DefinitionSummaries, _store.DefinitionSummaries.OrderBy(d => d.SortOrder).ThenBy(d => d.DisplayNameEn));
        Replace(CachedDefinitions, _store.Definitions.OrderBy(d => d.SortOrder).ThenBy(d => d.DisplayNameEn));
    }

    private static void Replace<T>(ObservableCollection<T> target, IEnumerable<T> source)
    {
        target.Clear();
        foreach (var item in source)
            target.Add(item);
    }
}
