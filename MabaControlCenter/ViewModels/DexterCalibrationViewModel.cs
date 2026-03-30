using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using MabaControlCenter.Services;

namespace MabaControlCenter.ViewModels;

public class DexterCalibrationViewModel : ViewModelBase
{
    private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = true };
    private static readonly string[] ProbeKeys =
    [
        "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12",
        "1", "2", "3", "4"
    ];

    private readonly IDeviceService _deviceService;
    private readonly ILoggingService _logging;
    private readonly INavigationService _navigationService;
    private MacroPadSession? _session;
    private string? _selectedMacroPort;
    private string? _listeningKeyId;
    private KeyEventHandler? _keyHandler;
    private bool _listenCtrl;
    private bool _listenAlt;
    private bool _listenShift;
    private bool _listenWin;
    private readonly List<string> _listenSeq = new();
    private Dictionary<string, int> _labelToIndex = new(StringComparer.OrdinalIgnoreCase);

    private readonly Dictionary<string, Dictionary<string, Dictionary<string, string>>> _profiles
        = new(StringComparer.OrdinalIgnoreCase);

    private string? _selectedProductVersion;
    private string? _selectedProfile;
    private string? _selectedGatePort;

    public DexterCalibrationViewModel(IDeviceService deviceService, ILoggingService logging, INavigationService navigationService)
    {
        _deviceService = deviceService;
        _logging = logging;
        _navigationService = navigationService;

        foreach (var id in MacroPadProtocol.UiOrder)
            KeyEntries.Add(new MacroPadKeyEntry(id));
        foreach (var e in KeyEntries)
            e.Attach(this);

        _deviceService.ConnectionStateChanged += (_, _) =>
        {
            RefreshGate();
            LoadGatePorts();
        };

        BackToModulesCommand = new RelayCommand(_ => { EndListen(); _session?.Dispose(); _session = null; _navigationService.NavigateTo("Modules"); });
        RefreshPortsCommand = new RelayCommand(_ => LoadMacroPorts());
        ConnectMacroCommand = new RelayCommand(_ => ConnectMacro(), _ => CanUseModule && !string.IsNullOrWhiteSpace(SelectedMacroPort) && (_session == null || !_session.IsOpen));
        DisconnectMacroCommand = new RelayCommand(_ => DisconnectMacro(), _ => _session?.IsOpen == true);
        LoadConfigCommand = new RelayCommand(_ => LoadFromDevice(), _ => CanUseModule && _session?.IsOpen == true);
        SaveConfigCommand = new RelayCommand(_ => SaveToDevice(), _ => CanUseModule && _session?.IsOpen == true);
        ApplyConfigCommand = new RelayCommand(_ => ApplyToDevice(), _ => CanUseModule && _session?.IsOpen == true);
        DefaultsCommand = new RelayCommand(_ => RunDefaults(), _ => CanUseModule && _session?.IsOpen == true);
        AutoMapCommand = new RelayCommand(_ => RunAutoMap(), _ => CanUseModule && _session?.IsOpen == true);
        CalibrateLayoutCommand = new RelayCommand(_ => RunCalibrateLayout(), _ => CanUseModule && _session?.IsOpen == true);
        ListenForKeyCommand = new RelayCommand(p => ToggleListen(p as string ?? p?.ToString()), _ => CanUseModule);
        AddProfileCommand = new RelayCommand(_ => AddProfile(), _ => !string.IsNullOrWhiteSpace(SelectedProductVersion));
        DeleteProfileCommand = new RelayCommand(_ => DeleteProfile(), _ => !string.IsNullOrWhiteSpace(SelectedProductVersion) && !string.IsNullOrWhiteSpace(SelectedProfile) && !MacroPadBuiltinProfiles.IsBuiltin(SelectedProfile) && !string.Equals(SelectedProfile, "Default", StringComparison.OrdinalIgnoreCase));
        FindAndConnectDexterCommand = new RelayCommand(_ => FindAndConnectDexter(), _ => CanFindAndConnectDexter());
        RefreshGatePortsCommand = new RelayCommand(_ => LoadGatePorts());

        ModuleLogEntries = new ObservableCollection<string>();
        GatePorts = new ObservableCollection<string>();
        ProductVersions = new ObservableCollection<string>();
        Profiles = new ObservableCollection<string>();
        LoadMacroPorts();
        LoadGatePorts();
        LoadHardwareLayout();
        ApplyLabelIndices();
        LoadProfileStore();
        InitializeProductAndProfiles();
        AddLog("MacroPad (legacy protocol). Connect port, then Load/Apply/Save.");
        RefreshGate();
    }

    public ObservableCollection<MacroPadKeyEntry> KeyEntries { get; } = new();
    public ObservableCollection<string> MacroPorts { get; } = new();
    public ObservableCollection<string> ProductVersions { get; }
    public ObservableCollection<string> Profiles { get; }

    public ICommand BackToModulesCommand { get; }
    public ICommand RefreshPortsCommand { get; }
    public ICommand ConnectMacroCommand { get; }
    public ICommand DisconnectMacroCommand { get; }
    public ICommand LoadConfigCommand { get; }
    public ICommand SaveConfigCommand { get; }
    public ICommand ApplyConfigCommand { get; }
    public ICommand DefaultsCommand { get; }
    public ICommand AutoMapCommand { get; }
    public ICommand CalibrateLayoutCommand { get; }
    public ICommand ListenForKeyCommand { get; }
    public ICommand AddProfileCommand { get; }
    public ICommand DeleteProfileCommand { get; }
    public ICommand FindAndConnectDexterCommand { get; }
    public ICommand RefreshGatePortsCommand { get; }

    public ObservableCollection<string> GatePorts { get; }

    public string? SelectedGatePort
    {
        get => _selectedGatePort;
        set
        {
            if (_selectedGatePort == value) return;
            _selectedGatePort = value;
            OnPropertyChanged();
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public ObservableCollection<string> ModuleLogEntries { get; }

    public bool CanUseModule => _deviceService.IsConnected && IsDexterProduct();
    public bool ShowDexterRequired => !CanUseModule;
    public string MacroSerialStatus => _session?.IsOpen == true ? "Connected" : "Disconnected";

    public string? ListeningKeyId
    {
        get => _listeningKeyId;
        private set
        {
            if (_listeningKeyId == value) return;
            _listeningKeyId = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsListening));
            foreach (var e in KeyEntries)
                e.RefreshListenUi();
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public bool IsListening => !string.IsNullOrEmpty(_listeningKeyId);

    public string? SelectedProductVersion
    {
        get => _selectedProductVersion;
        set
        {
            if (_selectedProductVersion == value) return;
            var prevVer = _selectedProductVersion;
            var prevProf = _selectedProfile;
            FlushProfileToStore(prevVer, prevProf);
            _selectedProductVersion = value;
            OnPropertyChanged();
            RefreshProfilesForProductAfterVersionChange();
        }
    }

    public string? SelectedProfile
    {
        get => _selectedProfile;
        set
        {
            if (_selectedProfile == value) return;
            var prevProf = _selectedProfile;
            var ver = _selectedProductVersion;
            FlushProfileToStore(ver, prevProf);
            _selectedProfile = value;
            OnPropertyChanged();
            LoadProfileToUi();
        }
    }

    public void OnKeyMappingCommitted()
    {
        if (string.IsNullOrWhiteSpace(SelectedProductVersion) || string.IsNullOrWhiteSpace(SelectedProfile))
            return;
        if (!_profiles.TryGetValue(SelectedProductVersion, out var perProduct))
        {
            perProduct = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
            _profiles[SelectedProductVersion] = perProduct;
        }
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var e in KeyEntries)
            map[e.Id] = e.Mapping ?? "";
        perProduct[SelectedProfile] = map;
        SaveProfileStore();
    }

    public string? SelectedMacroPort
    {
        get => _selectedMacroPort;
        set
        {
            if (_selectedMacroPort == value) return;
            _selectedMacroPort = value;
            OnPropertyChanged();
            CommandManager.InvalidateRequerySuggested();
        }
    }

    private bool IsDexterProduct()
    {
        var code = _deviceService.ConnectedProductCode;
        return !string.IsNullOrEmpty(code) && code.Contains("DEXTER", StringComparison.OrdinalIgnoreCase);
    }

    private void RefreshGate()
    {
        if (CanUseModule)
        {
            var code = _deviceService.ConnectedProductCode;
            if (!string.IsNullOrWhiteSpace(code) && !ProductVersions.Contains(code))
                ProductVersions.Add(code);
        }
        OnPropertyChanged(nameof(CanUseModule));
        OnPropertyChanged(nameof(ShowDexterRequired));
        CommandManager.InvalidateRequerySuggested();
    }

    private bool CanFindAndConnectDexter()
    {
        if (CanUseModule) return false;
        if (_deviceService.IsConnected && IsDexterProduct()) return false;
        return _deviceService.IsSimulationMode || !string.IsNullOrWhiteSpace(SelectedGatePort);
    }

    private void LoadGatePorts()
    {
        GatePorts.Clear();
        foreach (var p in _deviceService.GetAvailablePorts())
            GatePorts.Add(p);
        if (GatePorts.Count == 0 && _deviceService.IsSimulationMode)
        {
            for (var i = 1; i <= 6; i++)
                GatePorts.Add($"COM{i}");
        }
        if ((SelectedGatePort == null || !GatePorts.Contains(SelectedGatePort)) && GatePorts.Count > 0)
            SelectedGatePort = GatePorts[0];
    }

    private void FindAndConnectDexter()
    {
        try
        {
            if (_deviceService.IsConnected && !IsDexterProduct())
                _deviceService.Disconnect();

            if (_deviceService.IsConnected && IsDexterProduct())
            {
                RefreshGate();
                return;
            }

            var port = SelectedGatePort?.Trim() ?? "";
            if (!_deviceService.IsSimulationMode && string.IsNullOrEmpty(port))
            {
                MessageBox.Show(Application.Current.MainWindow, "Select a COM port for your Dexter.", "Find & connect Dexter",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            _deviceService.Connect(_deviceService.IsSimulationMode ? (port.Length > 0 ? port : "COM1") : port);
            if (_deviceService.IsSimulationMode)
                _ = _deviceService.PerformHandshake();
            else
                _deviceService.AssumeDexterForMacroPadModule();

            LoadMacroPorts();
            LoadGatePorts();
            RefreshGate();
            if (CanUseModule)
                AddLog("Dexter connected — you can use MacroPad below.");
        }
        catch (Exception ex)
        {
            MessageBox.Show(Application.Current.MainWindow, ex.Message, "Find & connect Dexter",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public void OnViewUnloaded()
    {
        EndListen();
    }

    private static string LayoutPath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MabaControlCenter", "macropad_layout.json");

    private static string ProfilesPath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MabaControlCenter", "macropad_profiles.json");

    private void LoadProfileStore()
    {
        try
        {
            if (!File.Exists(ProfilesPath)) return;
            var json = File.ReadAllText(ProfilesPath);
            var doc = JsonSerializer.Deserialize<ProfilesDto>(json);
            _profiles.Clear();
            if (doc?.Products != null)
            {
                foreach (var p in doc.Products)
                {
                    if (string.IsNullOrWhiteSpace(p.ProductCode)) continue;
                    var perProduct = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
                    if (p.Profiles != null)
                    {
                        foreach (var prof in p.Profiles)
                        {
                            if (string.IsNullOrWhiteSpace(prof.Name)) continue;
                            perProduct[prof.Name] = prof.Map ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        }
                    }
                    _profiles[p.ProductCode] = perProduct;
                }
            }
        }
        catch
        {
            // ignore bad file, start clean
            _profiles.Clear();
        }

        foreach (var per in _profiles.Values)
        {
            MacroPadBuiltinProfiles.EnsureBuiltins(per);
            if (!per.ContainsKey("Default"))
                per["Default"] = FullIdentityMap();
        }
        try
        {
            SaveProfileStore();
        }
        catch { /* ignore */ }
    }

    private void SaveProfileStore()
    {
        try
        {
            var dir = Path.GetDirectoryName(ProfilesPath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            var dto = new ProfilesDto
            {
                Products = _profiles.Select(p => new ProductProfilesDto
                {
                    ProductCode = p.Key,
                    Profiles = p.Value.Select(pr => new ProfileDto
                    {
                        Name = pr.Key,
                        Map = pr.Value
                    }).ToList()
                }).ToList()
            };

            File.WriteAllText(ProfilesPath, JsonSerializer.Serialize(dto, JsonOpts));
        }
        catch
        {
            // ignore persistence errors
        }
    }

    private static Dictionary<string, string> FullIdentityMap()
    {
        var d = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var id in MacroPadProtocol.UiOrder)
            d[id] = id;
        return d;
    }

    private void FlushProfileToStore(string? productVersion, string? profileName)
    {
        if (string.IsNullOrWhiteSpace(productVersion) || string.IsNullOrWhiteSpace(profileName))
            return;
        if (!_profiles.TryGetValue(productVersion, out var perProduct))
        {
            perProduct = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
            _profiles[productVersion] = perProduct;
        }
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var id in MacroPadProtocol.UiOrder)
            map[id] = "";
        foreach (var e in KeyEntries)
            map[e.Id] = e.Mapping ?? "";
        perProduct[profileName] = map;
        SaveProfileStore();
    }

    private void InitializeProductAndProfiles()
    {
        ProductVersions.Clear();
        foreach (var key in _profiles.Keys.OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
            ProductVersions.Add(key);

        var connected = _deviceService.ConnectedProductCode;
        if (!string.IsNullOrWhiteSpace(connected) && !ProductVersions.Contains(connected))
            ProductVersions.Add(connected);

        if (ProductVersions.Count == 0)
            ProductVersions.Add("DEXTER");

        if (string.IsNullOrWhiteSpace(SelectedProductVersion))
            SelectedProductVersion = ProductVersions[0];
        else
            RefreshProfilesForProductAfterVersionChange();
    }

    private void RefreshProfilesForProductAfterVersionChange()
    {
        Profiles.Clear();
        if (string.IsNullOrWhiteSpace(SelectedProductVersion))
            return;

        if (!_profiles.TryGetValue(SelectedProductVersion, out var perProduct))
        {
            perProduct = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
            _profiles[SelectedProductVersion] = perProduct;
        }

        MacroPadBuiltinProfiles.EnsureBuiltins(perProduct);
        if (!perProduct.ContainsKey("Default"))
            perProduct["Default"] = FullIdentityMap();
        MacroPadBuiltinProfiles.FillProfilesList(perProduct, Profiles);
        SaveProfileStore();

        var target = !string.IsNullOrWhiteSpace(_selectedProfile) && Profiles.Contains(_selectedProfile)
            ? _selectedProfile!
            : Profiles[0];
        _selectedProfile = target;
        OnPropertyChanged(nameof(SelectedProfile));
        LoadProfileToUi();
    }

    private void LoadProfileToUi()
    {
        if (string.IsNullOrWhiteSpace(SelectedProductVersion) || string.IsNullOrWhiteSpace(SelectedProfile))
            return;

        if (!_profiles.TryGetValue(SelectedProductVersion, out var perProduct))
            return;
        if (!perProduct.TryGetValue(SelectedProfile, out var stored) || stored == null)
            stored = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var norm = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var kv in stored)
            norm[kv.Key.ToLowerInvariant()] = kv.Value ?? "";

        var allSlotA = MacroPadProtocol.UiOrder.All(id =>
            norm.TryGetValue(id, out var v) &&
            string.Equals((v ?? "").Trim(), "a", StringComparison.OrdinalIgnoreCase));
        if (allSlotA)
        {
            foreach (var id in MacroPadProtocol.UiOrder)
                norm[id] = id;
            perProduct[SelectedProfile] = new Dictionary<string, string>(norm, StringComparer.OrdinalIgnoreCase);
            SaveProfileStore();
        }

        var isDefault = string.Equals(SelectedProfile, "Default", StringComparison.OrdinalIgnoreCase);
        foreach (var e in KeyEntries)
        {
            var id = e.Id;
            if (!norm.TryGetValue(id, out var val))
                val = "";
            if (string.IsNullOrEmpty(val) && isDefault)
                val = id;
            e.SetMappingFromProfile(val);
        }
    }

    private void SaveCurrentProfileSnapshot()
    {
        FlushProfileToStore(SelectedProductVersion, SelectedProfile);
    }

    private void AddProfile()
    {
        if (string.IsNullOrWhiteSpace(SelectedProductVersion))
            return;

        FlushProfileToStore(SelectedProductVersion, SelectedProfile);

        if (!_profiles.TryGetValue(SelectedProductVersion, out var perProduct))
        {
            perProduct = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
            _profiles[SelectedProductVersion] = perProduct;
        }

        var baseName = "Profile";
        var idx = 1;
        string name;
        do
        {
            name = $"{baseName} {idx}";
            idx++;
        } while (perProduct.ContainsKey(name));

        var clone = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var e in KeyEntries)
            clone[e.Id] = e.Mapping ?? "";
        perProduct[name] = clone;
        SaveProfileStore();

        Profiles.Add(name);
        _selectedProfile = name;
        OnPropertyChanged(nameof(SelectedProfile));
        LoadProfileToUi();
        AddLog($"Profile '{name}' created (cloned mapping).");
    }

    private void DeleteProfile()
    {
        if (string.IsNullOrWhiteSpace(SelectedProductVersion) || string.IsNullOrWhiteSpace(SelectedProfile))
            return;
        if (MacroPadBuiltinProfiles.IsBuiltin(SelectedProfile) || string.Equals(SelectedProfile, "Default", StringComparison.OrdinalIgnoreCase))
        {
            AddLog("Built-in and Default profiles cannot be deleted.");
            return;
        }

        FlushProfileToStore(SelectedProductVersion, SelectedProfile);

        if (!_profiles.TryGetValue(SelectedProductVersion, out var perProduct))
            return;

        if (!perProduct.Remove(SelectedProfile))
            return;

        var removed = SelectedProfile;
        Profiles.Remove(removed);
        SaveProfileStore();
        AddLog($"Profile '{removed}' deleted for {SelectedProductVersion}.");

        if (Profiles.Count == 0)
        {
            var def = "Default";
            Profiles.Add(def);
            perProduct[def] = FullIdentityMap();
            SaveProfileStore();
        }

        _selectedProfile = Profiles[0];
        OnPropertyChanged(nameof(SelectedProfile));
        LoadProfileToUi();
    }

    private void LoadHardwareLayout()
    {
        try
        {
            if (File.Exists(LayoutPath))
            {
                var json = File.ReadAllText(LayoutPath);
                var doc = JsonSerializer.Deserialize<LayoutDto>(json);
                if (doc?.Layout is { Count: 4 } rows && rows.All(r => r.Count == 4))
                {
                    _labelToIndex = MacroPadProtocol.LayoutToLabelToIndex(rows.Select(r => r.ToArray()).ToArray());
                    AddLog("Hardware layout loaded from file.");
                    return;
                }
            }
        }
        catch { /* use default */ }

        _labelToIndex = MacroPadProtocol.LayoutToLabelToIndex(MacroPadProtocol.LayoutHardwareDefault);
    }

    private void PersistLayout(string[][] rows)
    {
        try
        {
            var dir = Path.GetDirectoryName(LayoutPath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);
            File.WriteAllText(LayoutPath, JsonSerializer.Serialize(new LayoutDto { Layout = rows.Select(r => r.ToList()).ToList() }, JsonOpts));
        }
        catch { /* ignore */ }
    }

    private void ApplyLabelIndices()
    {
        foreach (var e in KeyEntries)
        {
            if (_labelToIndex.TryGetValue(e.Id, out var idx))
                e.PhysicalIndex = idx;
        }
    }

    private void LoadMacroPorts()
    {
        MacroPorts.Clear();
        foreach (var p in _deviceService.GetAvailablePorts())
            MacroPorts.Add(p);
        if (MacroPorts.Count == 0 && _deviceService.IsSimulationMode)
        {
            for (var i = 1; i <= 6; i++)
                MacroPorts.Add($"COM{i}");
        }
        if (SelectedMacroPort == null && MacroPorts.Count > 0)
            SelectedMacroPort = MacroPorts[0];
    }

    private void ConnectMacro()
    {
        _session?.Dispose();
        var sim = _deviceService.IsSimulationMode;
        _session = new MacroPadSession(_logging, sim);
        var port = SelectedMacroPort?.Trim() ?? "";
        if (string.IsNullOrEmpty(port))
        {
            AddLog("Select a port.");
            _session.Dispose();
            _session = null;
            return;
        }
        if (!_session.Open(port))
        {
            AddLog($"Open failed on {port} (need PONG @ 115200). Simulation: use Devices sim + Connect here.");
            _session.Dispose();
            _session = null;
            OnPropertyChanged(nameof(MacroSerialStatus));
            CommandManager.InvalidateRequerySuggested();
            return;
        }
        AddLog($"Connected {port} ({(sim ? "simulated protocol" : "115200")}). Profile mapping unchanged — click Load to read device into this profile.");
        OnPropertyChanged(nameof(MacroSerialStatus));
        CommandManager.InvalidateRequerySuggested();
    }

    private void DisconnectMacro()
    {
        EndListen();
        _session?.Dispose();
        _session = null;
        AddLog("Disconnected.");
        OnPropertyChanged(nameof(MacroSerialStatus));
        CommandManager.InvalidateRequerySuggested();
    }

    private MacroPadKeyEntry? EntryForPhysicalIndex(int index) =>
        KeyEntries.FirstOrDefault(e => e.PhysicalIndex == index);

    private void LoadFromDevice()
    {
        if (_session == null || !_session.IsOpen) return;
        try
        {
            var lines = _session.GetFromDevice();
            foreach (var ln in lines)
            {
                if (!MacroPadProtocol.TryParseMapLine(ln, out var idx, out var mods, out var keys))
                    continue;
                var ent = EntryForPhysicalIndex(idx);
                if (ent != null)
                    ent.SetMappingFromProfile(MacroPadProtocol.FormatShortcutMulti(mods, keys));
            }
            AddLog("Config loaded from device (GET).");
            SaveCurrentProfileSnapshot();
        }
        catch (Exception ex)
        {
            AddLog($"Load failed: {ex.Message}");
        }
    }

    private void ApplyToDevice()
    {
        if (_session == null || !_session.IsOpen) return;
        try
        {
            for (var i = 0; i < MacroPadProtocol.BtnCount; i++)
            {
                var ent = EntryForPhysicalIndex(i);
                var text = ent?.Mapping?.Trim() ?? "";
                if (string.IsNullOrEmpty(text))
                    throw new InvalidOperationException($"Shortcut empty for physical index {i}.");
                var (mods, keys) = MacroPadProtocol.ParseShortcutMulti(text);
                foreach (var k in keys)
                {
                    if (k.Contains(' '))
                        throw new InvalidOperationException($"Invalid token at {i}: spaces.");
                }
                _session.SetButton(i, mods, keys);
            }
            AddLog("Applied (not saved to EEPROM until Save).");
            SaveCurrentProfileSnapshot();
        }
        catch (Exception ex)
        {
            AddLog($"Apply failed: {ex.Message}");
            MessageBox.Show(Application.Current.MainWindow, ex.Message, "Apply", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void SaveToDevice()
    {
        if (_session == null || !_session.IsOpen) return;
        try
        {
            _session.SaveToDevice();
            AddLog("Saved to EEPROM.");
            SaveCurrentProfileSnapshot();
        }
        catch (Exception ex)
        {
            AddLog($"Save failed: {ex.Message}");
            MessageBox.Show(Application.Current.MainWindow, ex.Message, "Save", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void RunDefaults()
    {
        if (_session == null || !_session.IsOpen) return;
        try
        {
            _session.DefaultsOnDevice();
            _session.SaveToDevice();
            LoadFromDevice();
            AddLog("Defaults + SAVE + reload.");
            SaveCurrentProfileSnapshot();
        }
        catch (Exception ex)
        {
            AddLog($"Defaults failed: {ex.Message}");
            MessageBox.Show(Application.Current.MainWindow, ex.Message, "Defaults", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void RunAutoMap()
    {
        for (var i = 0; i < MacroPadProtocol.UiOrder.Length; i++)
        {
            var lbl = MacroPadProtocol.UiOrder[i];
            var ent = KeyEntries.FirstOrDefault(x => x.Id == lbl);
            ent?.SetMappingFromProfile(((char)('a' + i)).ToString());
        }
        AddLog("Auto A→P (letters); press Apply then Save.");
        FlushProfileToStore(SelectedProductVersion, SelectedProfile);
    }

    private void RunCalibrateLayout()
    {
        if (_session == null || !_session.IsOpen) return;
        List<string>? backup = null;
        try
        {
            backup = _session.GetFromDevice();
        }
        catch { /* ignore */ }

        try
        {
            _session.SendProbeKeys(ProbeKeys);
        }
        catch (Exception ex)
        {
            MessageBox.Show(Application.Current.MainWindow, ex.Message, "Calibrate", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var tok2idx = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < ProbeKeys.Length; i++)
            tok2idx[ProbeKeys[i].ToLowerInvariant()] = i;

        var usedIdx = new HashSet<int>();
        var mapping = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var step = 0;
        var owner = Application.Current.MainWindow;

        var win = new Window
        {
            Title = "Calibrate Layout",
            Width = 420,
            Height = 200,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = owner,
            Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xff, 0xff, 0xff))
        };
        var prompt = new System.Windows.Controls.TextBlock
        {
            Text = $"Step 1/16 – press position '{MacroPadProtocol.UiOrder[0]}' on MacroPad",
            Margin = new Thickness(16),
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            TextWrapping = TextWrapping.Wrap
        };
        var sub = new System.Windows.Controls.TextBlock
        {
            Text = "Use MacroPad keys only (mapped to F1…F12, 1–4 during probe). Esc = cancel.",
            Margin = new Thickness(16, 0, 16, 8),
            Foreground = System.Windows.Media.Brushes.Gray
        };
        var bar = new System.Windows.Controls.ProgressBar { Height = 20, Margin = new Thickness(16), Maximum = 16, Value = 0 };
        win.Content = new System.Windows.Controls.StackPanel { Children = { prompt, sub, bar } };
        win.Loaded += (_, __) => win.Focus();
        win.KeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape)
            {
                win.Close();
                return;
            }
            var tok = MacroPadProtocol.KeyToToken(e.Key);
            if (string.IsNullOrEmpty(tok)) return;
            var key = tok.ToLowerInvariant();
            if (key == "kp_add" || key == "oemplus") key = "+";
            if (!tok2idx.TryGetValue(key == "+" ? "+" : key, out var idx) && !tok2idx.TryGetValue(tok, out idx))
            {
                if (tok.Length == 1 && char.IsDigit(tok[0]) && tok2idx.TryGetValue(tok.ToLowerInvariant(), out var di))
                    idx = di;
                else
                    return;
            }
            if (usedIdx.Contains(idx)) return;
            e.Handled = true;
            var lbl = MacroPadProtocol.UiOrder[step];
            mapping[lbl] = idx;
            usedIdx.Add(idx);
            step++;
            bar.Value = step;
            if (step >= 16)
            {
                try
                {
                    var rows = MacroPadProtocol.IndexPermutationToRows(mapping);
                    _labelToIndex = MacroPadProtocol.LayoutToLabelToIndex(rows);
                    PersistLayout(rows);
                    ApplyLabelIndices();
                    if (backup != null)
                    {
                        foreach (var ln in backup)
                        {
                            if (!MacroPadProtocol.TryParseMapLine(ln, out var pi, out var mods, out var keys))
                                continue;
                            try { _session.SetButton(pi, mods, keys); } catch { /* ignore restore errors */ }
                        }
                    }
                    AddLog("Calibration done. Layout saved.");
                }
                catch (Exception ex)
                {
                    AddLog($"Calibrate finish: {ex.Message}");
                }
                win.Close();
            }
            else
            {
                prompt.Text = $"Step {step + 1}/16 – press '{MacroPadProtocol.UiOrder[step]}'";
            }
        };
        win.ShowDialog();
    }

    private void ToggleListen(string? keyId)
    {
        if (string.IsNullOrEmpty(keyId)) return;
        if (ListeningKeyId == keyId)
        {
            EndListen();
            return;
        }
        EndListen();
        ListeningKeyId = keyId;
        _listenCtrl = _listenAlt = _listenShift = _listenWin = false;
        _listenSeq.Clear();
        _keyHandler = (_, e) =>
        {
            if (ListeningKeyId == null) return;
            e.Handled = true;
            if (e.Key == Key.Escape)
            {
                EndListen();
                AddLog("Listen cancelled.");
                return;
            }
            if (e.Key == Key.Enter)
            {
                var mods = (_listenCtrl ? 1 : 0) | (_listenAlt ? 2 : 0) | (_listenShift ? 4 : 0) | (_listenWin ? 8 : 0);
                var keys = _listenSeq.Count > 0 ? _listenSeq.ToList() : new List<string> { "A" };
                var text = MacroPadProtocol.FormatShortcutMulti(mods, keys);
                var ent = KeyEntries.FirstOrDefault(x => x.Id == ListeningKeyId);
                if (ent != null) ent.Mapping = text;
                AddLog($"{ent?.Label} → {text}");
                EndListen();
                return;
            }
            if (e.Key == Key.Back)
            {
                if (_listenSeq.Count > 0) _listenSeq.RemoveAt(_listenSeq.Count - 1);
                UpdateListenPreview();
                return;
            }
            if (e.Key is Key.LeftCtrl or Key.RightCtrl) { _listenCtrl = true; UpdateListenPreview(); return; }
            if (e.Key is Key.LeftAlt or Key.RightAlt) { _listenAlt = true; UpdateListenPreview(); return; }
            if (e.Key is Key.LeftShift or Key.RightShift) { _listenShift = true; UpdateListenPreview(); return; }
            if (e.Key is Key.LWin or Key.RWin) { _listenWin = true; UpdateListenPreview(); return; }

            var tok = MacroPadProtocol.KeyToToken(e.Key);
            if (string.IsNullOrEmpty(tok)) return;
            if (_listenSeq.Count == 0 || _listenSeq[^1] != tok)
                _listenSeq.Add(tok);
            UpdateListenPreview();
        };
        var w = Application.Current?.MainWindow;
        if (w != null) w.PreviewKeyDown += _keyHandler;
        AddLog($"Listening… Enter=finish, Esc=cancel, Backspace=undo ({keyId.ToUpperInvariant()})");
    }

    private void UpdateListenPreview()
    {
        var mods = (_listenCtrl ? 1 : 0) | (_listenAlt ? 2 : 0) | (_listenShift ? 4 : 0) | (_listenWin ? 8 : 0);
        var text = MacroPadProtocol.FormatShortcutMulti(mods, _listenSeq);
        var ent = KeyEntries.FirstOrDefault(x => x.Id == ListeningKeyId);
        if (ent == null) return;
        if (_listenSeq.Count > 0 || _listenCtrl || _listenAlt || _listenShift || _listenWin)
            ent.Mapping = string.IsNullOrEmpty(text) ? "" : text;
    }

    private void EndListen()
    {
        if (_keyHandler != null)
        {
            var w = Application.Current?.MainWindow;
            if (w != null) w.PreviewKeyDown -= _keyHandler;
        }
        _keyHandler = null;
        ListeningKeyId = null;
        _listenSeq.Clear();
    }

    private void AddLog(string message)
    {
        var ts = DateTime.Now.ToString("HH:mm:ss");
        ModuleLogEntries.Insert(0, $"[{ts}] {message}");
    }

    private sealed class LayoutDto
    {
        public List<List<string>>? Layout { get; set; }
    }

    private sealed class ProfilesDto
    {
        public List<ProductProfilesDto>? Products { get; set; }
    }

    private sealed class ProductProfilesDto
    {
        public string ProductCode { get; set; } = string.Empty;
        public List<ProfileDto>? Profiles { get; set; }
    }

    private sealed class ProfileDto
    {
        public string Name { get; set; } = string.Empty;
        public Dictionary<string, string>? Map { get; set; }
    }
}
