using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public class MachineCatalogService : IMachineCatalogService
{
    private static readonly JsonSerializerOptions JsonOptions = MachinePlatformJson.CreateOptions();
    private static readonly Guid LocalCategoryId = Guid.Parse("7b2f1d90-c76b-4f54-a7d4-8d85e95af101");
    private static readonly Guid LocalFamilyId = Guid.Parse("7b2f1d90-c76b-4f54-a7d4-8d85e95af102");
    private static readonly Guid LocalDefinitionId = Guid.Parse("7b2f1d90-c76b-4f54-a7d4-8d85e95af103");

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

            NormalizeImageUrls(summaries, definitions);

            _store = new MachineDefinitionCacheStore
            {
                Categories = categories,
                Families = families,
                DefinitionSummaries = summaries,
                Definitions = definitions,
                LastSyncedAt = DateTime.UtcNow,
                LastSyncStatus = $"Synced {definitions.Count} machine definition(s)."
            };
            EnsureLocalFallbackEntries();
            SaveCache();
            PopulateCollections();
        }
        catch (Exception ex)
        {
            _store.LastSyncStatus = CachedDefinitions.Count > 0
                ? $"Backend unavailable. Using cached machine definitions. {ex.Message}"
                : $"Backend unavailable and no machine definition cache exists. {ex.Message}";
            EnsureLocalFallbackEntries();
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

            NormalizeImageUrl(definition);
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
        try
        {
            return await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions, cancellationToken);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Machine catalog JSON parse failed for {typeof(T).Name}. Path: {ex.Path}. {ex.Message}", ex);
        }
    }

    private void LoadCache()
    {
        try
        {
            if (File.Exists(_cacheFilePath))
            {
                var json = File.ReadAllText(_cacheFilePath);
                _store = JsonSerializer.Deserialize<MachineDefinitionCacheStore>(json, JsonOptions) ?? new MachineDefinitionCacheStore();
                NormalizeImageUrls(_store.DefinitionSummaries, _store.Definitions);
            }
        }
        catch (JsonException ex)
        {
            _store = new MachineDefinitionCacheStore { LastSyncStatus = $"Machine definition cache enum/JSON parse failed. Path: {ex.Path}. {ex.Message}" };
        }
        catch
        {
            _store = new MachineDefinitionCacheStore { LastSyncStatus = "Machine definition cache could not be read." };
        }

        EnsureLocalFallbackEntries();
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
        EnsureLocalFallbackEntries();
        NormalizeImageUrls(_store.DefinitionSummaries, _store.Definitions);
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

    private void NormalizeImageUrls(IEnumerable<MachineDefinitionSummary> summaries, IEnumerable<MachineDefinition> definitions)
    {
        foreach (var summary in summaries)
            NormalizeImageUrl(summary);

        foreach (var definition in definitions)
            NormalizeImageUrl(definition);
    }

    private void NormalizeImageUrl(MachineDefinitionSummary summary)
    {
        summary.ThumbnailUrl = NormalizeImageUrlValue(summary.ThumbnailUrl);
        summary.ImageUrl = NormalizeImageUrlValue(summary.ImageUrl);
        summary.ImageSource = NormalizeImageUrlValue(summary.ImageSource);
    }

    private void NormalizeImageUrl(MachineDefinition definition)
    {
        definition.ThumbnailUrl = NormalizeImageUrlValue(definition.ThumbnailUrl);
        definition.ImageUrl = NormalizeImageUrlValue(definition.ImageUrl);
        definition.ImageSource = NormalizeImageUrlValue(definition.ImageSource);
    }

    private string? NormalizeImageUrlValue(string? raw)
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

    private void EnsureLocalFallbackEntries()
    {
        if (_store.Categories.All(c => c.Id != LocalCategoryId))
        {
            _store.Categories.Add(new MachineCategory
            {
                Id = LocalCategoryId,
                Code = "LOCAL-CNC",
                DisplayNameEn = "Local CNC",
                DisplayNameAr = "ماكينة CNC محلية",
                DescriptionEn = "Local fallback CNC category for the current Arduino-based machine.",
                DescriptionAr = "فئة CNC محلية احتياطية للماكينة الحالية المعتمدة على أردوينو.",
                SortOrder = -100,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        if (_store.Families.All(f => f.Id != LocalFamilyId))
        {
            _store.Families.Add(new MachineFamily
            {
                Id = LocalFamilyId,
                CategoryId = LocalCategoryId,
                CategoryDisplayNameEn = "Local CNC",
                Code = "MABA-CNC-SERIES",
                DisplayNameEn = "MABA CNC Series",
                DisplayNameAr = "سلسلة MABA CNC",
                DescriptionEn = "Fallback local machine family for the current Arduino CNC hardware.",
                DescriptionAr = "عائلة محلية احتياطية للماكينة الحالية من نوع Arduino CNC.",
                Manufacturer = "MABA",
                IsActive = true,
                SortOrder = -100,
                CreatedAt = DateTime.UtcNow
            });
        }

        if (_store.Definitions.All(d => d.Id != LocalDefinitionId))
            _store.Definitions.Add(BuildLocalFallbackDefinition());

        if (_store.DefinitionSummaries.All(s => s.Id != LocalDefinitionId))
            _store.DefinitionSummaries.Add(BuildLocalFallbackSummary());
    }

    private static MachineDefinitionSummary BuildLocalFallbackSummary()
    {
        return new MachineDefinitionSummary
        {
            Id = LocalDefinitionId,
            Code = "MABA-ARDUINO-CNC",
            Version = "1.0.0",
            CategoryId = LocalCategoryId,
            CategoryDisplayNameEn = "Local CNC",
            FamilyId = LocalFamilyId,
            FamilyDisplayNameEn = "MABA CNC Series",
            DisplayNameEn = "MABA Arduino CNC - Current Machine",
            DisplayNameAr = "ماكينة MABA Arduino CNC الحالية",
            DescriptionEn = "Current Arduino-based CNC machine profile used for real hardware and simulation testing.",
            DescriptionAr = "ملف تعريف الماكينة الحالية المعتمدة على أردوينو للاختبار الحقيقي والمحاكاة.",
            Manufacturer = "MABA",
            IsActive = true,
            IsPublic = false,
            IsDeprecated = false,
            SortOrder = -100,
            ReleasedAt = DateTime.UtcNow,
            Tags = new List<string> { "CNC", "G-code", "3-axis", "Local" },
            DefaultDriverType = DriverType.ArduinoSerial.ToString(),
            SupportedSetupModes = new List<string> { SetupMode.RealOnly.ToString(), SetupMode.SimulationOnly.ToString() },
            RuntimeUiVariant = "cnc-standard-v1",
            CreatedAt = DateTime.UtcNow
        };
    }

    private static MachineDefinition BuildLocalFallbackDefinition()
    {
        return new MachineDefinition
        {
            Id = LocalDefinitionId,
            Code = "MABA-ARDUINO-CNC",
            Version = "1.0.0",
            CategoryId = LocalCategoryId,
            CategoryDisplayNameEn = "Local CNC",
            FamilyId = LocalFamilyId,
            FamilyDisplayNameEn = "MABA CNC Series",
            DisplayNameEn = "MABA Arduino CNC - Current Machine",
            DisplayNameAr = "ماكينة MABA Arduino CNC الحالية",
            DescriptionEn = "Current Arduino-based CNC machine profile used for real hardware and simulation testing.",
            DescriptionAr = "ملف تعريف الماكينة الحالية المعتمدة على أردوينو للاختبار الحقيقي والمحاكاة.",
            Manufacturer = "MABA",
            Tags = new List<string> { "CNC", "G-code", "3-axis", "Local" },
            IsActive = true,
            IsPublic = false,
            SortOrder = -100,
            ReleasedAt = DateTime.UtcNow,
            RuntimeBinding = new RuntimeBindingSection
            {
                DefaultDriverType = DriverType.ArduinoSerial,
                SupportedDriverTypes = new List<DriverType> { DriverType.ArduinoSerial, DriverType.Simulated },
                FirmwareProtocol = FirmwareProtocol.Custom,
                SupportedSetupModes = new List<SetupMode> { SetupMode.RealOnly, SetupMode.SimulationOnly },
                VisualizationType = VisualizationType.CncTopDown2D,
                KinematicsType = KinematicsType.MovingGantryXY,
                RuntimeUiVariant = "cnc-standard-v1"
            },
            AxisConfig = new AxisConfigSection
            {
                AxisCount = 3,
                SupportedAxes = new List<AxisId> { AxisId.X, AxisId.Y, AxisId.Z },
                AxisRoles = new Dictionary<string, AxisRole> { ["X"] = AxisRole.Primary, ["Y"] = AxisRole.Secondary, ["Z"] = AxisRole.Vertical },
                AxisDirections = new Dictionary<string, Direction> { ["X"] = Direction.Normal, ["Y"] = Direction.Normal, ["Z"] = Direction.Normal },
                HomingSupport = new Dictionary<string, bool> { ["X"] = true, ["Y"] = true, ["Z"] = false },
                HomeOriginConvention = MachineHomeOriginConvention.FrontLeft,
                WorkCoordinateSupport = true,
                MachineCoordinateSupport = true,
                RelativeMoveSupport = true,
                AbsoluteMoveSupport = true
            },
            Workspace = new WorkspaceSection
            {
                MinTravelMm = new Dictionary<string, double> { ["X"] = 0, ["Y"] = 0, ["Z"] = 0 },
                MaxTravelMm = new Dictionary<string, double> { ["X"] = 300, ["Y"] = 300, ["Z"] = 100 },
                WorkAreaMm = new WorkAreaDimensions { Width = 300, Depth = 300, Height = 100 }
            },
            MotionDefaults = new MotionDefaultsSection
            {
                StepsPerMm = new Dictionary<string, double> { ["X"] = 80, ["Y"] = 80, ["Z"] = 400 },
                MaxFeedMmMin = new Dictionary<string, double> { ["X"] = 1200, ["Y"] = 1200, ["Z"] = 300 },
                JogPresets = new List<JogPreset>
                {
                    new() { Label = "0.1 mm", DistanceMm = 0.1, FeedMmMin = 300 },
                    new() { Label = "1 mm", DistanceMm = 1, FeedMmMin = 600 },
                    new() { Label = "10 mm", DistanceMm = 10, FeedMmMin = 900 }
                }
            },
            ConnectionDefaults = new ConnectionDefaultsSection
            {
                DefaultBaudRate = 115200,
                SupportedBaudRates = new List<int> { 115200 },
                SupportedConnectionTypes = new List<ConnectionType> { ConnectionType.Serial, ConnectionType.Simulated },
                RequiresHandshake = false,
                CommandTerminator = "\n",
                ResponseAckPattern = "HOME DONE",
                ProtocolNotes = "Legacy step-based protocol using +stepsx/-stepsx, H for homing and B for square test."
            },
            Capabilities = new CapabilitiesSection
            {
                Motion = new MotionCapabilities
                {
                    Homing = true,
                    ZHoming = false,
                    CombinedXYHoming = true,
                    RelativeMoves = true,
                    AbsoluteMoves = false,
                    Pause = false,
                    Resume = false,
                    Stop = false,
                    Park = false,
                    CenterMove = true,
                    WorkOffset = true,
                    JogContinuous = false,
                    JogStep = true
                },
                Execution = new ExecutionCapabilities
                {
                    RealExecution = true,
                    Simulation = true,
                    PreviewPlayback = true,
                    DryRun = true,
                    FileRun = true,
                    Frame = true,
                    BoundingBoxPreview = true,
                    LiveReportedPosition = false,
                    EstimatedPositionOnly = true,
                    ToolpathPreview = true,
                    ProgressTracking = true
                },
                Protocol = new ProtocolCapabilities
                {
                    Handshake = false,
                    Acknowledgements = false,
                    AlarmReporting = false,
                    AlarmReset = false,
                    StatusQuery = false,
                    PositionQuery = false,
                    MotorEnable = false,
                    MotorDisable = false,
                    FeedHold = false,
                    SoftReset = false
                },
                Visualization = new VisualizationCapabilities
                {
                    MachineVisualization = true,
                    TopView2D = true,
                    Perspective3D = true,
                    KinematicsAnimation = true,
                    RealTimePositionDisplay = true
                },
                FileHandling = new FileHandlingCapabilities
                {
                    LocalFileRun = true,
                    StreamingExecution = true,
                    GcodeValidation = true,
                    MultipleFileFormats = true
                }
            },
            FileSupport = new FileSupportSection
            {
                SupportedInputFileTypes = new List<string> { ".gcode", ".nc", ".txt" },
                GcodeDialect = GcodeDialect.MabaCustom,
                SupportedOperationTypes = new List<OperationType> { OperationType.Milling, OperationType.Engraving, OperationType.Drilling, OperationType.Plotting }
            },
            Visualization = new VisualizationSection
            {
                VisualizationType = VisualizationType.CncTopDown2D,
                KinematicsType = KinematicsType.MovingGantryXY,
                CoordinatePresentationMode = CoordinateMode.TopLeft,
                MachineShapeHint = MachineShapeHint.Rectangular,
                DefaultViewMode = ViewMode.Top2D
            },
            ProfileRules = new ProfileRulesSection
            {
                AllowedOverrides = new List<OverrideField> { OverrideField.DriverType, OverrideField.BaudRate, OverrideField.StepsPerMm, OverrideField.JogPresets, OverrideField.Notes },
                BuiltInProfileRules = new BuiltInProfileRules { IsEditable = false, IsDeletable = false, IsDuplicatable = true },
                UserProfileRules = new UserProfileRules { IsEditable = true, IsDeletable = true, IsDuplicatable = true }
            }
        };
    }
}
