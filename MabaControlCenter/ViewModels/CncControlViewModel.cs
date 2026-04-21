using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using MabaControlCenter.Models;
using MabaControlCenter.Services;
using Microsoft.Win32;

namespace MabaControlCenter.ViewModels;

public class CncControlViewModel : ViewModelBase
{
    private readonly ICncControllerService _cncControllerService;
    private readonly ICncProfileService _cncProfileService;
    private readonly IActiveProductionJobService _activeProductionJobService;
    private readonly INavigationService _navigationService;
    private readonly IGcodeParserService _gcodeParserService;
    private readonly ICncExecutionQueueService _executionQueueService;
    private readonly ICncJobSessionService _jobSessionService;
    private readonly ICncPreviewPlaybackService _previewPlaybackService;
    private readonly ICncFramePathService _framePathService;
    private readonly ICncJobPlacementService _jobPlacementService;
    private readonly IMachineCatalogService _machineCatalogService;
    private readonly IRuntimeProfileService _runtimeProfileService;
    private readonly IActiveMachineContextService _activeMachineContextService;

    private string? _selectedPort;
    private decimal _selectedJogStep = 1m;
    private string _lastFeedback = "CNC controller ready.";
    private int _baudRate = 115200;
    private decimal _xStepsPerMm;
    private decimal _yStepsPerMm;
    private decimal _zStepsPerMm;
    private decimal _xMinMm;
    private decimal _xLimitMm;
    private decimal _yMinMm;
    private decimal _yLimitMm;
    private decimal _zMinMm;
    private decimal _zLimitMm;
    private string _profileName = string.Empty;
    private string _profileDescription = string.Empty;
    private string _profileNotes = string.Empty;
    private bool _softLimitsEnabled = true;
    private bool _homeXEnabled = true;
    private bool _homeYEnabled = true;
    private bool _homeZEnabled;
    private bool _supportsXAxis = true;
    private bool _supportsYAxis = true;
    private bool _supportsZAxis = true;
    private CncDriverType _selectedDriverType = CncDriverType.ArduinoSerial;
    private CncHomeOriginConvention _selectedHomeOrigin = CncHomeOriginConvention.TopLeft;
    private string _jogPresetsText = "0.1, 1, 10";
    private CncMachineProfile? _selectedProfile;
    private GcodeParseResult? _loadedProgram;
    private string _lastError = "No current alarms.";
    private double _previewPlaybackSpeed = 1d;
    private CncFrameBounds _frameBounds = new();
    private readonly List<GcodeMotionCommand> _placedMotions = new();
    private CncJobPlacement _jobPlacement = new();
    private decimal _placementOffsetX;
    private decimal _placementOffsetY;
    private string _placementStatus = "Placement offsets are zeroed.";
    private MachineCategory? _selectedMachineCategory;
    private MachineFamily? _selectedMachineFamily;
    private MachineDefinitionSummary? _selectedMachineDefinitionSummary;
    private RuntimeProfile? _selectedRuntimeProfile;
    private string _newRuntimeProfileName = string.Empty;
    private string _machinePlatformStatus = "Machine platform cache not synced yet.";
    private bool _isMachineWizardOpen;
    private bool _isProfileManagerOpen;
    private MachineWizardStep _wizardStep = MachineWizardStep.Catalog;
    private MachineWizardCard? _wizardSelectedFamilyCard;
    private MachineWizardCard? _wizardSelectedMachineCard;
    private MachineWizardCard? _wizardSelectedModeCard;
    private SetupMode? _wizardSelectedSetupMode;

    public CncControlViewModel(
        ICncControllerService cncControllerService,
        ICncProfileService cncProfileService,
        IActiveProductionJobService activeProductionJobService,
        INavigationService navigationService,
        IGcodeParserService gcodeParserService,
        ICncExecutionQueueService executionQueueService,
        ICncJobSessionService jobSessionService,
        ICncPreviewPlaybackService previewPlaybackService,
        ICncFramePathService framePathService,
        ICncJobPlacementService jobPlacementService,
        IMachineCatalogService machineCatalogService,
        IRuntimeProfileService runtimeProfileService,
        IActiveMachineContextService activeMachineContextService)
    {
        _cncControllerService = cncControllerService;
        _cncProfileService = cncProfileService;
        _activeProductionJobService = activeProductionJobService;
        _navigationService = navigationService;
        _gcodeParserService = gcodeParserService;
        _executionQueueService = executionQueueService;
        _jobSessionService = jobSessionService;
        _previewPlaybackService = previewPlaybackService;
        _framePathService = framePathService;
        _jobPlacementService = jobPlacementService;
        _machineCatalogService = machineCatalogService;
        _runtimeProfileService = runtimeProfileService;
        _activeMachineContextService = activeMachineContextService;

        AvailablePorts = new ObservableCollection<string>();
        JogStepPresets = new ObservableCollection<decimal>();
        DiagnosticsMessages = new ObservableCollection<string>();
        WizardSteps = new ObservableCollection<MachineWizardStepItem>();
        WizardFamilyCards = new ObservableCollection<MachineWizardCard>();
        WizardMachineCards = new ObservableCollection<MachineWizardCard>();
        WizardModeCards = new ObservableCollection<MachineWizardCard>();
        DriverTypes = Enum.GetValues(typeof(CncDriverType)).Cast<CncDriverType>().ToList();
        HomeOriginOptions = Enum.GetValues(typeof(CncHomeOriginConvention)).Cast<CncHomeOriginConvention>().ToList();

        RefreshPortsCommand = new RelayCommand(_ => RefreshPorts());
        ConnectCommand = new RelayCommand(_ => RunAction(() => _cncControllerService.Connect(SelectedPort ?? string.Empty)), _ => !IsConnected && (IsSimulationMode || !string.IsNullOrWhiteSpace(SelectedPort)));
        DisconnectCommand = new RelayCommand(_ => RunAction(() => _cncControllerService.Disconnect()), _ => IsConnected);
        EnableMotorsCommand = new RelayCommand(_ => RunResponseAction(() => _cncControllerService.EnableMotors()), _ => IsConnected);
        DisableMotorsCommand = new RelayCommand(_ => RunResponseAction(() => _cncControllerService.DisableMotors()), _ => IsConnected);
        HomeCommand = new RelayCommand(_ => RunResponseAction(() => _cncControllerService.AutoHome()), _ => IsConnected);
        GoToCenterCommand = new RelayCommand(_ => RunResponseAction(() => _cncControllerService.GoToCenter()), _ => CanGoToCenter);
        SetZeroCommand = new RelayCommand(_ => RunResponseAction(() => _cncControllerService.SetWorkZero()), _ => IsConnected);
        ResetStateCommand = new RelayCommand(_ => RunResponseAction(() => _cncControllerService.ResetState()), _ => CanResetState);
        ClearWarningCommand = new RelayCommand(_ => RunAction(_cncControllerService.ClearWarning), _ => HasWarning);
        StopCommand = new RelayCommand(async _ => await StopExecutionAsync(), _ => CanStopExecution);
        RefreshStatusCommand = new RelayCommand(_ => RunResponseAction(() => _cncControllerService.RefreshStatus()), _ => IsConnected);
        SaveConfigCommand = new RelayCommand(_ => SaveConfig(), _ => CanSaveSelectedProfile);
        ApplyProfileCommand = new RelayCommand(_ => ApplySelectedProfile(), _ => CanApplySelectedProfile);
        DuplicateProfileCommand = new RelayCommand(_ => DuplicateSelectedProfile(), _ => SelectedProfile != null);
        DeleteProfileCommand = new RelayCommand(_ => DeleteSelectedProfile(), _ => CanDeleteSelectedProfile);
        RestoreDefaultProfilesCommand = new RelayCommand(_ => RestoreDefaultProfiles());
        SyncMachineCatalogCommand = new RelayCommand(async _ => await SyncMachineCatalogAsync());
        CreateRuntimeProfileCommand = new RelayCommand(async _ => await CreateRuntimeProfileFromSelectionAsync(), _ => SelectedMachineDefinitionSummary != null);
        OpenMachineWizardCommand = new RelayCommand(_ => OpenMachineWizard());
        CloseMachineWizardCommand = new RelayCommand(_ => CloseMachineWizard());
        WizardBackCommand = new RelayCommand(_ => MoveWizardBack(), _ => CanMoveWizardBack);
        WizardNextCommand = new RelayCommand(async _ => await MoveWizardNextAsync(), _ => CanMoveWizardNext);
        SelectWizardFamilyCommand = new RelayCommand(SelectWizardFamily);
        SelectWizardMachineCommand = new RelayCommand(SelectWizardMachine);
        SelectWizardModeCommand = new RelayCommand(SelectWizardMode);
        ConfirmMachineWizardCommand = new RelayCommand(async _ => await ConfirmMachineWizardAsync(), _ => WizardSelectedMachineDefinition != null);
        ManageProfilesCommand = new RelayCommand(_ => IsProfileManagerOpen = !IsProfileManagerOpen);
        UseSelectedMachineCommand = new RelayCommand(_ => ConfirmAndActivateSelectedMachine(), _ => SelectedMachineDefinitionSummary != null && SelectedRuntimeProfile != null);
        ActivateRuntimeProfileCommand = new RelayCommand(_ => ActivateSelectedRuntimeProfile(), _ => SelectedRuntimeProfile != null);
        DuplicateRuntimeProfileCommand = new RelayCommand(_ => DuplicateSelectedRuntimeProfile(), _ => SelectedRuntimeProfile != null);
        DeleteRuntimeProfileCommand = new RelayCommand(_ => DeleteSelectedRuntimeProfile(), _ => CanDeleteSelectedRuntimeProfile);
        BackToJobsCommand = new RelayCommand(_ => _navigationService.NavigateTo("Jobs"), _ => ActiveJob != null);
        JogXPositiveCommand = new RelayCommand(_ => Jog("X", SelectedJogStep), _ => CanJog);
        JogXNegativeCommand = new RelayCommand(_ => Jog("X", -SelectedJogStep), _ => CanJog);
        JogYPositiveCommand = new RelayCommand(_ => Jog("Y", SelectedJogStep), _ => CanJog);
        JogYNegativeCommand = new RelayCommand(_ => Jog("Y", -SelectedJogStep), _ => CanJog);
        JogZPositiveCommand = new RelayCommand(_ => Jog("Z", SelectedJogStep), _ => CanJog);
        JogZNegativeCommand = new RelayCommand(_ => Jog("Z", -SelectedJogStep), _ => CanJog);
        LoadGcodeCommand = new RelayCommand(_ => LoadGcodeFile());
        ClearLoadedProgramCommand = new RelayCommand(_ => ClearLoadedProgram(), _ => HasLoadedProgram);
        ApplyPlacementCommand = new RelayCommand(_ => ApplyPlacement(), _ => CanApplyPlacement);
        ResetPlacementCommand = new RelayCommand(_ => ResetPlacement(), _ => CanApplyPlacement);
        PlaceTopLeftCommand = new RelayCommand(_ => ApplyPlacementPreset(CncPlacementPreset.TopLeft), _ => CanApplyPlacement);
        PlaceTopRightCommand = new RelayCommand(_ => ApplyPlacementPreset(CncPlacementPreset.TopRight), _ => CanApplyPlacement);
        PlaceBottomLeftCommand = new RelayCommand(_ => ApplyPlacementPreset(CncPlacementPreset.BottomLeft), _ => CanApplyPlacement);
        PlaceBottomRightCommand = new RelayCommand(_ => ApplyPlacementPreset(CncPlacementPreset.BottomRight), _ => CanApplyPlacement);
        PlaceCenterCommand = new RelayCommand(_ => ApplyPlacementPreset(CncPlacementPreset.Center), _ => CanApplyPlacement);
        FramePreviewCommand = new RelayCommand(_ => StartFramePreview(), _ => CanFramePreview);
        PlayPreviewCommand = new RelayCommand(_ => PlayPreview(), _ => CanPlayPreview);
        PausePreviewCommand = new RelayCommand(_ => PausePreview(), _ => CanPausePreview);
        StopPreviewCommand = new RelayCommand(_ => StopPreview(), _ => CanStopPreview);
        StartProgramCommand = new RelayCommand(async _ => await StartProgramAsync(), _ => CanStartProgram);
        PauseProgramCommand = new RelayCommand(_ => PauseProgram(), _ => CanPauseProgram);
        ResumeProgramCommand = new RelayCommand(async _ => await ResumeProgramAsync(), _ => CanResumeProgram);

        _cncControllerService.StateChanged += (_, _) => RefreshState();
        _cncControllerService.ConnectionLost += (_, _) =>
        {
            _jobSessionService.InterruptSession(CompletedMotionCount, TotalMotionCount, MachineX, MachineY, MachineZ, "Execution interrupted by serial disconnect.");
            AddDiagnostic("Error", "Serial disconnect detected. Execution was interrupted and motion commands were halted.");
            LastFeedback = "Serial disconnect detected.";
            RefreshState();
        };
        _cncProfileService.ActiveProfileChanged += (_, _) =>
        {
            LoadProfileFields(_cncProfileService.ActiveProfile);
            RefreshState();
        };
        _cncProfileService.ProfilesChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(Profiles));
            if (SelectedProfile == null || !_cncProfileService.Profiles.Any(p => p.ProfileId == SelectedProfile.ProfileId))
                LoadProfileFields(_cncProfileService.ActiveProfile);
            CommandManager.InvalidateRequerySuggested();
        };
        _activeProductionJobService.CurrentJobChanged += (_, _) => RefreshActiveJob();
        _runtimeProfileService.ProfilesChanged += (_, _) => RefreshRuntimeProfiles();
        _runtimeProfileService.ActiveProfileChanged += (_, _) => RefreshActiveMachineContext();
        _jobSessionService.SessionChanged += (_, _) => RefreshWorkflowState();
        _previewPlaybackService.PlaybackChanged += (_, _) => RefreshPreviewState();
        _executionQueueService.ExecutionStateChanged += (_, _) =>
        {
            if (!string.IsNullOrWhiteSpace(_executionQueueService.LastInterruptionReason))
            {
                var severity = _executionQueueService.ExecutionState == CncExecutionState.Error ? "Error" : "Info";
                AddDiagnostic(severity, _executionQueueService.LastInterruptionReason);
            }

            SyncSessionWithExecutionState();
            RefreshExecutionState();
        };

        LoadProfileFields(_cncProfileService.ActiveProfile);
        RefreshActiveMachineContext();
        RefreshPorts();
        RefreshActiveJob();
        RefreshWorkflowState();
        RefreshState();
    }

    public string Title => "CNC Control Panel";
    public string Subtitle => "Manual control and safer G-code execution for the Arduino-based CNC machine.";
    public ObservableCollection<string> AvailablePorts { get; }
    public ObservableCollection<decimal> JogStepPresets { get; }
    public ObservableCollection<LogEntry> SerialLogs => _cncControllerService.SerialLogs;
    public ObservableCollection<string> DiagnosticsMessages { get; }
    public ObservableCollection<CncOperatorEventEntry> OperatorEvents => _jobSessionService.OperatorEvents;
    public IEnumerable<CncMachineProfile> Profiles => _cncProfileService.Profiles;
    public IEnumerable<RuntimeProfile> RuntimeProfiles => _runtimeProfileService.Profiles
        .Where(profile => SelectedMachineDefinitionSummary == null || profile.MachineDefinitionId == SelectedMachineDefinitionSummary.Id)
        .OrderBy(profile => profile.ProfileType == RuntimeProfileType.System ? 0 : 1)
        .ThenBy(profile => profile.ProfileName);
    public IEnumerable<MachineCategory> MachineCategories => _machineCatalogService.Categories;
    public IEnumerable<MachineFamily> MachineFamilies => _machineCatalogService.Families
        .Where(f => SelectedMachineCategory == null || f.CategoryId == SelectedMachineCategory.Id);
    public IEnumerable<MachineDefinitionSummary> MachineDefinitionSummaries => _machineCatalogService.DefinitionSummaries
        .Where(d => SelectedMachineCategory == null || d.CategoryId == SelectedMachineCategory.Id)
        .Where(d => SelectedMachineFamily == null || d.FamilyId == SelectedMachineFamily.Id);
    public ObservableCollection<MachineWizardStepItem> WizardSteps { get; }
    public ObservableCollection<MachineWizardCard> WizardFamilyCards { get; }
    public ObservableCollection<MachineWizardCard> WizardMachineCards { get; }
    public ObservableCollection<MachineWizardCard> WizardModeCards { get; }
    public IReadOnlyList<CncDriverType> DriverTypes { get; }
    public IReadOnlyList<CncHomeOriginConvention> HomeOriginOptions { get; }
    public string MachineCatalogSyncStatus => _machineCatalogService.LastSyncStatus;
    public string MachinePlatformStatus
    {
        get => _machinePlatformStatus;
        private set { if (_machinePlatformStatus == value) return; _machinePlatformStatus = value; OnPropertyChanged(); }
    }
    private CapabilitiesSection EffectiveCapabilities => _activeMachineContextService.Current.EffectiveCapabilities;
    public string ActiveMachineContextText => _activeMachineContextService.Current.StatusText;
    public string EffectiveCapabilitiesSummary => BuildEffectiveCapabilitiesSummary();
    public bool HasSelectedRuntimeProfile => SelectedRuntimeProfile != null;
    public bool HasSelectedMachineDefinition => SelectedMachineDefinitionSummary != null;
    public bool IsMachineWizardOpen
    {
        get => _isMachineWizardOpen;
        set { if (_isMachineWizardOpen == value) return; _isMachineWizardOpen = value; OnPropertyChanged(); }
    }
    public bool IsProfileManagerOpen
    {
        get => _isProfileManagerOpen;
        set { if (_isProfileManagerOpen == value) return; _isProfileManagerOpen = value; OnPropertyChanged(); }
    }
    public MachineWizardStep WizardStep
    {
        get => _wizardStep;
        set
        {
            if (_wizardStep == value) return;
            _wizardStep = value;
            RefreshWizardStepState();
            OnPropertyChanged();
        }
    }
    public bool IsWizardCatalogStep => WizardStep == MachineWizardStep.Catalog;
    public bool IsWizardFamilyStep => WizardStep == MachineWizardStep.Family;
    public bool IsWizardMachineStep => WizardStep == MachineWizardStep.Machine;
    public bool IsWizardModeStep => WizardStep == MachineWizardStep.Mode;
    public bool IsWizardConfirmStep => WizardStep == MachineWizardStep.Confirm;
    public bool CanMoveWizardBack => WizardStep != MachineWizardStep.Catalog;
    public bool CanMoveWizardNext => WizardStep switch
    {
        MachineWizardStep.Catalog => _machineCatalogService.Families.Count > 0 && _machineCatalogService.DefinitionSummaries.Count > 0,
        MachineWizardStep.Family => WizardSelectedMachineFamily != null,
        MachineWizardStep.Machine => WizardSelectedMachineDefinition != null,
        MachineWizardStep.Mode => WizardSelectedSetupMode != null,
        MachineWizardStep.Confirm => WizardSelectedMachineDefinition != null,
        _ => false
    };
    public string WizardNextButtonText => WizardStep == MachineWizardStep.Confirm ? "Confirm" : "Next";
    public string WizardSyncDisplay => _machineCatalogService.LastSyncedAt.HasValue
        ? $"Last sync: {_machineCatalogService.LastSyncedAt:yyyy-MM-dd HH:mm}"
        : "Not synced";
    public string WizardConnectivityText => _machineCatalogService.DefinitionSummaries.Count > 0 ? "Online / Cached" : "Offline";
    public bool HasWizardMachines => _machineCatalogService.DefinitionSummaries.Count > 0;
    public MachineFamily? WizardSelectedMachineFamily => _wizardSelectedFamilyCard?.Family;
    public MachineDefinitionSummary? WizardSelectedMachineDefinition => _wizardSelectedMachineCard?.MachineSummary;
    public SetupMode? WizardSelectedSetupMode
    {
        get => _wizardSelectedSetupMode;
        set { if (_wizardSelectedSetupMode == value) return; _wizardSelectedSetupMode = value; OnPropertyChanged(); }
    }

    public MachineCategory? SelectedMachineCategory
    {
        get => _selectedMachineCategory;
        set
        {
            if (_selectedMachineCategory?.Id == value?.Id) return;
            _selectedMachineCategory = value;
            _selectedMachineFamily = null;
            _selectedMachineDefinitionSummary = null;
            _selectedRuntimeProfile = null;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SelectedMachineFamily));
            OnPropertyChanged(nameof(SelectedMachineDefinitionSummary));
            OnPropertyChanged(nameof(SelectedRuntimeProfile));
            OnPropertyChanged(nameof(HasSelectedMachineDefinition));
            OnPropertyChanged(nameof(HasSelectedRuntimeProfile));
            OnPropertyChanged(nameof(MachineFamilies));
            OnPropertyChanged(nameof(MachineDefinitionSummaries));
            OnPropertyChanged(nameof(RuntimeProfiles));
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public MachineFamily? SelectedMachineFamily
    {
        get => _selectedMachineFamily;
        set
        {
            if (_selectedMachineFamily?.Id == value?.Id) return;
            _selectedMachineFamily = value;
            _selectedMachineDefinitionSummary = null;
            _selectedRuntimeProfile = null;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SelectedMachineDefinitionSummary));
            OnPropertyChanged(nameof(SelectedRuntimeProfile));
            OnPropertyChanged(nameof(HasSelectedMachineDefinition));
            OnPropertyChanged(nameof(HasSelectedRuntimeProfile));
            OnPropertyChanged(nameof(MachineDefinitionSummaries));
            OnPropertyChanged(nameof(RuntimeProfiles));
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public MachineDefinitionSummary? SelectedMachineDefinitionSummary
    {
        get => _selectedMachineDefinitionSummary;
        set
        {
            if (_selectedMachineDefinitionSummary?.Id == value?.Id) return;
            _selectedMachineDefinitionSummary = value;
            if (value != null)
                NewRuntimeProfileName = $"{value.DisplayNameEn} Custom";
            SelectPreferredRuntimeProfileForSelectedMachine();
            OnPropertyChanged();
            OnPropertyChanged(nameof(RuntimeProfiles));
            OnPropertyChanged(nameof(HasSelectedMachineDefinition));
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public RuntimeProfile? SelectedRuntimeProfile
    {
        get => _selectedRuntimeProfile;
        set
        {
            if (_selectedRuntimeProfile?.RuntimeProfileId == value?.RuntimeProfileId) return;
            _selectedRuntimeProfile = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasSelectedRuntimeProfile));
            OnPropertyChanged(nameof(CanDeleteSelectedRuntimeProfile));
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public string NewRuntimeProfileName
    {
        get => _newRuntimeProfileName;
        set { if (_newRuntimeProfileName == value) return; _newRuntimeProfileName = value; OnPropertyChanged(); }
    }
    public ICommand RefreshPortsCommand { get; }
    public ICommand ConnectCommand { get; }
    public ICommand DisconnectCommand { get; }
    public ICommand EnableMotorsCommand { get; }
    public ICommand DisableMotorsCommand { get; }
    public ICommand HomeCommand { get; }
    public ICommand GoToCenterCommand { get; }
    public ICommand SetZeroCommand { get; }
    public ICommand ResetStateCommand { get; }
    public ICommand ClearWarningCommand { get; }
    public ICommand StopCommand { get; }
    public ICommand RefreshStatusCommand { get; }
    public ICommand SaveConfigCommand { get; }
    public ICommand ApplyProfileCommand { get; }
    public ICommand DuplicateProfileCommand { get; }
    public ICommand DeleteProfileCommand { get; }
    public ICommand RestoreDefaultProfilesCommand { get; }
    public ICommand SyncMachineCatalogCommand { get; }
    public ICommand CreateRuntimeProfileCommand { get; }
    public ICommand OpenMachineWizardCommand { get; }
    public ICommand CloseMachineWizardCommand { get; }
    public ICommand WizardBackCommand { get; }
    public ICommand WizardNextCommand { get; }
    public ICommand SelectWizardFamilyCommand { get; }
    public ICommand SelectWizardMachineCommand { get; }
    public ICommand SelectWizardModeCommand { get; }
    public ICommand ConfirmMachineWizardCommand { get; }
    public ICommand ManageProfilesCommand { get; }
    public ICommand UseSelectedMachineCommand { get; }
    public ICommand ActivateRuntimeProfileCommand { get; }
    public ICommand DuplicateRuntimeProfileCommand { get; }
    public ICommand DeleteRuntimeProfileCommand { get; }
    public ICommand BackToJobsCommand { get; }
    public ICommand JogXPositiveCommand { get; }
    public ICommand JogXNegativeCommand { get; }
    public ICommand JogYPositiveCommand { get; }
    public ICommand JogYNegativeCommand { get; }
    public ICommand JogZPositiveCommand { get; }
    public ICommand JogZNegativeCommand { get; }
    public ICommand LoadGcodeCommand { get; }
    public ICommand ClearLoadedProgramCommand { get; }
    public ICommand ApplyPlacementCommand { get; }
    public ICommand ResetPlacementCommand { get; }
    public ICommand PlaceTopLeftCommand { get; }
    public ICommand PlaceTopRightCommand { get; }
    public ICommand PlaceBottomLeftCommand { get; }
    public ICommand PlaceBottomRightCommand { get; }
    public ICommand PlaceCenterCommand { get; }
    public ICommand FramePreviewCommand { get; }
    public ICommand PlayPreviewCommand { get; }
    public ICommand PausePreviewCommand { get; }
    public ICommand StopPreviewCommand { get; }
    public ICommand StartProgramCommand { get; }
    public ICommand PauseProgramCommand { get; }
    public ICommand ResumeProgramCommand { get; }

    public ActiveProductionJobContext? ActiveJob => _activeProductionJobService.CurrentJob?.MachineType == "CNC"
        ? _activeProductionJobService.CurrentJob
        : null;

    public bool HasActiveJob => ActiveJob != null;
    public string ActiveJobTitle => ActiveJob?.Title ?? "No production job loaded";
    public string ActiveJobReference => ActiveJob?.JobReference ?? "Open a CNC job from Jobs to attach it here.";
    public string ActiveProfileName => _cncProfileService.ActiveProfile.ProfileName;
    public string ActiveProfileDriver => _cncProfileService.ActiveProfile.DriverType.ToString();
    public string ActiveProfileBaudRateDisplay => _cncProfileService.ActiveProfile.DriverType == CncDriverType.Simulated
        ? "No physical baud rate required in simulation mode"
        : $"{_cncProfileService.ActiveProfile.BaudRate}";
    public string DriverCapabilitiesSummary => _cncControllerService.DriverCapabilities.Summary;
    public bool IsSimulationMode => _cncProfileService.ActiveProfile.DriverType == CncDriverType.Simulated;
    public bool IsHardwarePortRequired => !IsSimulationMode;
    public string RuntimeModeText => IsSimulationMode
        ? "Simulation Driver (offline CNC mode)"
        : "Arduino Serial Driver (hardware mode)";
    public CncLoadedJobInfo? LoadedJobInfo => _jobSessionService.LoadedJob;
    public CncJobSession? CurrentJobSession => _jobSessionService.CurrentSession;
    public CncJobSession? LastJobSessionSummary => _jobSessionService.LastCompletedSession;
    public CncJobLifecycleState JobSessionState => _jobSessionService.SessionState;
    public string JobSessionStateDisplay => FormatJobSessionState(JobSessionState);
    public string JobReadinessSummary => _jobSessionService.ReadinessSummary;
    public string JobBlockingReason => _jobSessionService.BlockingReason ?? "No blocking issues.";
    public bool HasBlockingReason => !string.IsNullOrWhiteSpace(_jobSessionService.BlockingReason);
    public bool HasLoadedJobInfo => LoadedJobInfo != null;
    public bool HasCurrentSession => CurrentJobSession != null;
    public bool HasLastJobSessionSummary => LastJobSessionSummary != null;
    public string LoadedJobTitle => LoadedJobInfo?.JobTitle ?? "No operator job loaded";
    public string LoadedJobSourceReference => LoadedJobInfo?.SourceReference ?? "Load a CNC file or open a CNC production job.";
    public string LoadedJobPath => LoadedJobInfo?.FilePath ?? "No file path available.";
    public string LoadedJobMotionCount => LoadedJobInfo != null ? $"{LoadedJobInfo.MotionLineCount} motion lines" : "0 motion lines";
    public string LoadedJobProfileName => LoadedJobInfo?.ActiveProfileName ?? ActiveProfileName;
    public string LoadedJobDriverMode => LoadedJobInfo?.DriverMode ?? RuntimeModeText;
    public string SessionStartedAtDisplay => CurrentJobSession?.StartedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Not started";
    public string SessionEndedAtDisplay => (CurrentJobSession?.EndedAt ?? CurrentJobSession?.InterruptedAt)?.ToString("yyyy-MM-dd HH:mm:ss") ?? "In progress";
    public string SessionLastAction => CurrentJobSession?.LastAction ?? "No session action yet";
    public string SessionResultText => CurrentJobSession?.ResultMessage ?? "No session result yet";
    public string SessionDurationDisplay => FormatDuration(CurrentJobSession?.Duration);
    public string SessionSummaryText => BuildSessionSummary();
    public bool IsReadyToRun => JobSessionState == CncJobLifecycleState.Ready;

    public CncMachineProfile? SelectedProfile
    {
        get => _selectedProfile;
        set
        {
            if (_selectedProfile?.ProfileId == value?.ProfileId)
                return;

            _selectedProfile = value;
            OnPropertyChanged();
            RefreshProfilePermissionState();
            if (value != null)
                LoadProfileFields(value);
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public string? SelectedPort
    {
        get => _selectedPort;
        set
        {
            if (_selectedPort == value) return;
            _selectedPort = value;
            OnPropertyChanged();
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public decimal SelectedJogStep
    {
        get => _selectedJogStep;
        set
        {
            if (_selectedJogStep == value) return;
            _selectedJogStep = value;
            OnPropertyChanged();
        }
    }

    public string ProfileName
    {
        get => _profileName;
        set { if (_profileName == value) return; _profileName = value; OnPropertyChanged(); }
    }

    public string ProfileDescription
    {
        get => _profileDescription;
        set { if (_profileDescription == value) return; _profileDescription = value; OnPropertyChanged(); }
    }

    public string ProfileNotes
    {
        get => _profileNotes;
        set { if (_profileNotes == value) return; _profileNotes = value; OnPropertyChanged(); }
    }

    public int BaudRate
    {
        get => _baudRate;
        set { if (_baudRate == value) return; _baudRate = value; OnPropertyChanged(); }
    }

    public bool SoftLimitsEnabled
    {
        get => _softLimitsEnabled;
        set { if (_softLimitsEnabled == value) return; _softLimitsEnabled = value; OnPropertyChanged(); }
    }

    public bool HomeXEnabled
    {
        get => _homeXEnabled;
        set { if (_homeXEnabled == value) return; _homeXEnabled = value; OnPropertyChanged(); }
    }

    public bool HomeYEnabled
    {
        get => _homeYEnabled;
        set { if (_homeYEnabled == value) return; _homeYEnabled = value; OnPropertyChanged(); }
    }

    public bool HomeZEnabled
    {
        get => _homeZEnabled;
        set { if (_homeZEnabled == value) return; _homeZEnabled = value; OnPropertyChanged(); }
    }

    public bool SupportsXAxis
    {
        get => _supportsXAxis;
        set { if (_supportsXAxis == value) return; _supportsXAxis = value; OnPropertyChanged(); }
    }

    public bool SupportsYAxis
    {
        get => _supportsYAxis;
        set { if (_supportsYAxis == value) return; _supportsYAxis = value; OnPropertyChanged(); }
    }

    public bool SupportsZAxis
    {
        get => _supportsZAxis;
        set { if (_supportsZAxis == value) return; _supportsZAxis = value; OnPropertyChanged(); }
    }

    public CncDriverType SelectedDriverType
    {
        get => _selectedDriverType;
        set { if (_selectedDriverType == value) return; _selectedDriverType = value; OnPropertyChanged(); }
    }

    public CncHomeOriginConvention SelectedHomeOrigin
    {
        get => _selectedHomeOrigin;
        set { if (_selectedHomeOrigin == value) return; _selectedHomeOrigin = value; OnPropertyChanged(); }
    }

    public string JogPresetsText
    {
        get => _jogPresetsText;
        set { if (_jogPresetsText == value) return; _jogPresetsText = value; OnPropertyChanged(); }
    }

    public bool IsConnected => _cncControllerService.IsConnected;
    public bool MotorsEnabled => _cncControllerService.MotorsEnabled;
    public bool IsHomed => _cncControllerService.IsHomed;
    public bool HasValidMachineReference => _cncControllerService.HasValidMachineReference;
    public bool HasAlarm => _cncControllerService.MachineState is CncMachineState.Alarm or CncMachineState.Error;
    public bool HasWarning => _cncControllerService.MachineState == CncMachineState.Warning || !string.IsNullOrWhiteSpace(_cncControllerService.LastWarning);
    public bool HasDiagnostics => DiagnosticsMessages.Count > 0;
    public string ConnectionStatusText => IsConnected
        ? $"Connected on {_cncControllerService.ConnectedPort}"
        : "Disconnected";
    public string MachineStateDisplay => FormatState(_cncControllerService.MachineState);

    public bool CanJog => IsConnected
                          && _cncControllerService.DeviceStatus.IsResponsive
                          && MotorsEnabled
                          && !HasAlarm
                          && EffectiveCapabilities.Motion.JogStep
                          && _executionQueueService.ExecutionState is not CncExecutionState.Running and not CncExecutionState.Paused
                          && _cncControllerService.MachineState is CncMachineState.Idle or CncMachineState.Running or CncMachineState.Stopped or CncMachineState.Warning;
    public bool CanGoToCenter => IsConnected
                                 && _cncControllerService.DeviceStatus.IsResponsive
                                 && MotorsEnabled
                                 && !HasAlarm
                                 && EffectiveCapabilities.Motion.CenterMove
                                 && _executionQueueService.ExecutionState is not CncExecutionState.Running and not CncExecutionState.Paused
                                 && SupportsXAxis
                                 && SupportsYAxis;

    public string LastFeedback
    {
        get => _lastFeedback;
        private set
        {
            if (_lastFeedback == value) return;
            _lastFeedback = value;
            OnPropertyChanged();
        }
    }

    public string LastError
    {
        get => _lastError;
        private set
        {
            if (_lastError == value) return;
            _lastError = value;
            OnPropertyChanged();
        }
    }

    public string CurrentAlarmText => _cncControllerService.LastFaultReason ?? "No active alarm.";
    public string CurrentWarningText => _cncControllerService.LastWarning ?? "No active warning.";
    public string DiagnosticsSummary => $"{TotalMotionCount} parsed moves | {WarningCount} warnings | {ErrorCount} errors";
    public int WarningCount => DiagnosticsMessages.Count(m => m.StartsWith("[Warning]"));
    public int ErrorCount => DiagnosticsMessages.Count(m => m.StartsWith("[Error]"));
    public string DeviceReadyDisplay => _cncControllerService.DeviceStatus.IsReady ? "Ready" : "Waiting / Not Ready";
    public string DeviceStateDisplay => FormatDeviceState(_cncControllerService.DeviceStatus.DeviceState);
    public string PositionTrackingDisplay => _cncControllerService.DeviceStatus.PositionTrackingMode;
    public string LastAcknowledgementText => _cncControllerService.DeviceStatus.LastAcknowledgement ?? "No acknowledgement received yet.";
    public string ProtocolStatusText => _cncControllerService.DeviceStatus.LastStatusText ?? "No protocol status received yet.";
    public string ProtocolErrorText => _cncControllerService.DeviceStatus.LastProtocolError ?? "No active protocol error.";
    public string DeviceReportedPositionText => _cncControllerService.DeviceStatus.HasReportedPosition
        ? $"X {_cncControllerService.DeviceStatus.ReportedX:0.###}, Y {_cncControllerService.DeviceStatus.ReportedY:0.###}, Z {_cncControllerService.DeviceStatus.ReportedZ:0.###}"
        : "No device-reported position yet.";

    public decimal MachineX => _cncControllerService.MachineX;
    public decimal MachineY => _cncControllerService.MachineY;
    public decimal MachineZ => _cncControllerService.MachineZ;
    public decimal WorkX => _cncControllerService.WorkX;
    public decimal WorkY => _cncControllerService.WorkY;
    public decimal WorkZ => _cncControllerService.WorkZ;
    public decimal WorkOffsetX => _cncControllerService.WorkOffsetX;
    public decimal WorkOffsetY => _cncControllerService.WorkOffsetY;
    public decimal WorkOffsetZ => _cncControllerService.WorkOffsetZ;
    public CncMachineState MachineStateValue => _cncControllerService.MachineState;
    public CncExecutionState ExecutionStateValue => _executionQueueService.ExecutionState;

    public decimal XStepsPerMm
    {
        get => _xStepsPerMm;
        set { if (_xStepsPerMm == value) return; _xStepsPerMm = value; OnPropertyChanged(); }
    }

    public decimal YStepsPerMm
    {
        get => _yStepsPerMm;
        set { if (_yStepsPerMm == value) return; _yStepsPerMm = value; OnPropertyChanged(); }
    }

    public decimal ZStepsPerMm
    {
        get => _zStepsPerMm;
        set { if (_zStepsPerMm == value) return; _zStepsPerMm = value; OnPropertyChanged(); }
    }

    public decimal XMinMm
    {
        get => _xMinMm;
        set { if (_xMinMm == value) return; _xMinMm = value; OnPropertyChanged(); }
    }

    public decimal XLimitMm
    {
        get => _xLimitMm;
        set { if (_xLimitMm == value) return; _xLimitMm = value; OnPropertyChanged(); }
    }

    public decimal YMinMm
    {
        get => _yMinMm;
        set { if (_yMinMm == value) return; _yMinMm = value; OnPropertyChanged(); }
    }

    public decimal YLimitMm
    {
        get => _yLimitMm;
        set { if (_yLimitMm == value) return; _yLimitMm = value; OnPropertyChanged(); }
    }

    public decimal ZMinMm
    {
        get => _zMinMm;
        set { if (_zMinMm == value) return; _zMinMm = value; OnPropertyChanged(); }
    }

    public decimal ZLimitMm
    {
        get => _zLimitMm;
        set { if (_zLimitMm == value) return; _zLimitMm = value; OnPropertyChanged(); }
    }

    public GcodeParseResult? LoadedProgram
    {
        get => _loadedProgram;
        private set
        {
            if (_loadedProgram == value) return;
            _loadedProgram = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasLoadedProgram));
            OnPropertyChanged(nameof(LoadedFileName));
            OnPropertyChanged(nameof(LoadedFileSummary));
            OnPropertyChanged(nameof(MotionCommands));
            OnPropertyChanged(nameof(TotalMotionCount));
            OnPropertyChanged(nameof(CanStartProgram));
            OnPropertyChanged(nameof(CanStopExecution));
            OnPropertyChanged(nameof(DiagnosticsSummary));
            OnPropertyChanged(nameof(LoadedJobMotionCount));
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public bool HasLoadedProgram => LoadedProgram != null;
    public string LoadedFileName => LoadedProgram?.FileName ?? "No G-code file loaded";
    public string LoadedFileSummary => LoadedProgram == null
        ? "Load a .gcode, .nc, or .txt file to build the execution queue."
        : $"{LoadedProgram.Motions.Count} motion line(s) from {LoadedProgram.TotalLines} file line(s)";
    public IEnumerable<GcodeMotionCommand> MotionCommands => _placedMotions;
    public int TotalMotionCount => MotionCommands.Count(m => m.IsExecutable);
    public int CurrentLineNumber => _executionQueueService.CurrentMotion?.LineNumber ?? 0;
    public string CurrentRawLine => _executionQueueService.CurrentMotion?.RawText ?? "No motion line executing.";
    public int CompletedMotionCount => _executionQueueService.CompletedCount;
    public int RemainingMotionCount => Math.Max(0, TotalMotionCount - CompletedMotionCount);
    public string ProgressSummary => $"{CompletedMotionCount} completed / {TotalMotionCount} total / {RemainingMotionCount} remaining";
    public string ExecutionStateDisplay => FormatExecutionState(_executionQueueService.ExecutionState);
    public int CurrentMotionIndex => _executionQueueService.CurrentMotionIndex;
    public IEnumerable<GcodeMotionCommand> PreviewMotions => _previewPlaybackService.ActivePlaybackMotions;
    public CncPreviewSimulationState PreviewSimulationState => _previewPlaybackService.SimulationState;
    public string PreviewSimulationStateDisplay => FormatPreviewSimulationState(PreviewSimulationState);
    public int PreviewCurrentSegmentIndex => _previewPlaybackService.CurrentSegmentIndex;
    public double PreviewSegmentProgress => _previewPlaybackService.SegmentProgress;
    public double PreviewToolX => (double)_previewPlaybackService.ToolX;
    public double PreviewToolY => (double)_previewPlaybackService.ToolY;
    public string PreviewCurrentLineText => PreviewCurrentSegmentIndex >= 0 && PreviewCurrentSegmentIndex < _previewPlaybackService.ActivePlaybackMotions.Count
        ? $"Line {_previewPlaybackService.ActivePlaybackMotions[PreviewCurrentSegmentIndex].LineNumber}: {_previewPlaybackService.ActivePlaybackMotions[PreviewCurrentSegmentIndex].RawText}"
        : "Preview is at the start position.";
    public decimal PreviewTotalDistanceMm => _previewPlaybackService.Summary.TotalDistanceMm;
    public decimal PreviewRapidDistanceMm => _previewPlaybackService.Summary.RapidDistanceMm;
    public decimal PreviewCutDistanceMm => _previewPlaybackService.Summary.CutDistanceMm;
    public string PreviewEstimatedTime => FormatDuration(_previewPlaybackService.Summary.EstimatedTime);
    public double PreviewPlaybackSpeed
    {
        get => _previewPlaybackSpeed;
        set
        {
            var clamped = Math.Clamp(value, 0.5d, 4d);
            if (Math.Abs(_previewPlaybackSpeed - clamped) < 0.001d)
                return;

            _previewPlaybackSpeed = clamped;
            _previewPlaybackService.SetPlaybackSpeed(clamped);
            OnPropertyChanged();
            OnPropertyChanged(nameof(PreviewPlaybackSpeedDisplay));
        }
    }
    public string PreviewPlaybackSpeedDisplay => $"{PreviewPlaybackSpeed:0.0}x";
    public bool IsFramePreviewActive => _previewPlaybackService.IsFramePlayback;
    public bool UsePreviewPlaybackInViewport => PreviewSimulationState != CncPreviewSimulationState.Idle;
    public bool HasFrameBounds => _frameBounds.IsValid;
    public decimal FrameMinX => _frameBounds.MinX;
    public decimal FrameMaxX => _frameBounds.MaxX;
    public decimal FrameMinY => _frameBounds.MinY;
    public decimal FrameMaxY => _frameBounds.MaxY;
    public bool CanFramePreview => HasLoadedProgram && HasFrameBounds && EffectiveCapabilities.Execution.Frame;
    public decimal PlacementOffsetX
    {
        get => _placementOffsetX;
        set
        {
            if (_placementOffsetX == value) return;
            _placementOffsetX = value;
            OnPropertyChanged();
        }
    }
    public decimal PlacementOffsetY
    {
        get => _placementOffsetY;
        set
        {
            if (_placementOffsetY == value) return;
            _placementOffsetY = value;
            OnPropertyChanged();
        }
    }
    public decimal AppliedPlacementOffsetX => _jobPlacement.OffsetX;
    public decimal AppliedPlacementOffsetY => _jobPlacement.OffsetY;
    public string PlacementStatus
    {
        get => _placementStatus;
        private set
        {
            if (_placementStatus == value) return;
            _placementStatus = value;
            OnPropertyChanged();
        }
    }
    public bool CanApplyPlacement => HasLoadedProgram && _executionQueueService.ExecutionState is not (CncExecutionState.Running or CncExecutionState.Paused);
    public bool CanPlayPreview => HasLoadedProgram && MotionCommands.Any() && EffectiveCapabilities.Execution.ToolpathPreview && EffectiveCapabilities.Execution.PreviewPlayback && PreviewSimulationState is CncPreviewSimulationState.Ready or CncPreviewSimulationState.Paused or CncPreviewSimulationState.Completed;
    public bool CanPausePreview => PreviewSimulationState == CncPreviewSimulationState.Playing;
    public bool CanStopPreview => PreviewSimulationState is CncPreviewSimulationState.Playing or CncPreviewSimulationState.Paused or CncPreviewSimulationState.Completed;
    public bool CanStartProgram =>
        JobSessionState is CncJobLifecycleState.Ready or CncJobLifecycleState.Completed or CncJobLifecycleState.Stopped or CncJobLifecycleState.Interrupted &&
        IsConnected &&
        _cncControllerService.DeviceStatus.IsResponsive &&
        MotorsEnabled &&
        !HasAlarm &&
        HasLoadedProgram &&
        TotalMotionCount > 0 &&
        ErrorCount == 0 &&
        EffectiveCapabilities.Execution.FileRun &&
        _executionQueueService.ExecutionState is CncExecutionState.Idle or CncExecutionState.Completed or CncExecutionState.Stopped;
    public bool CanPauseProgram => EffectiveCapabilities.Motion.Pause && _executionQueueService.ExecutionState == CncExecutionState.Running;
    public bool CanResumeProgram => EffectiveCapabilities.Motion.Resume && _executionQueueService.ExecutionState == CncExecutionState.Paused && IsConnected && MotorsEnabled && !HasAlarm;
    public bool CanStopExecution => _executionQueueService.ExecutionState is CncExecutionState.Running or CncExecutionState.Paused;
    public bool CanResetState => IsConnected && (HasAlarm || _cncControllerService.MachineState == CncMachineState.Warning);
    public bool CanApplySelectedProfile => SelectedProfile != null && SelectedProfile.ProfileId != _cncProfileService.ActiveProfile.ProfileId;
    public bool IsSelectedProfileBuiltIn => SelectedProfile?.IsBuiltIn == true;
    public bool CanEditSelectedProfile => SelectedProfile?.IsEditable == true;
    public bool CanSaveSelectedProfile => CanEditSelectedProfile;
    public bool CanDeleteSelectedProfile => SelectedProfile != null && SelectedProfile.IsEditable && !SelectedProfile.IsDefault && _cncProfileService.Profiles.Count > 1;
    public bool CanDeleteSelectedRuntimeProfile => SelectedRuntimeProfile?.ProfileType == RuntimeProfileType.User;
    public string SelectedProfileKindDisplay => IsSelectedProfileBuiltIn ? "System profile / Read-only" : "User profile / Editable";
    public string ProfilePermissionHint => IsSelectedProfileBuiltIn
        ? "This system profile is protected. Duplicate it to create an editable copy."
        : "This user-created profile can be edited, saved, duplicated, or deleted.";

    private void RefreshPorts()
    {
        AvailablePorts.Clear();
        if (IsSimulationMode)
        {
            AvailablePorts.Add("SIMULATION");
            SelectedPort = "SIMULATION";
            return;
        }

        foreach (var port in _cncControllerService.GetAvailablePorts())
            AvailablePorts.Add(port);

        if (!string.IsNullOrWhiteSpace(SelectedPort) && AvailablePorts.Contains(SelectedPort))
            return;

        SelectedPort = AvailablePorts.FirstOrDefault();
    }

    private void RunAction(Action action)
    {
        try
        {
            action();
            LastFeedback = "Command completed.";
            RefreshState();
        }
        catch (Exception ex)
        {
            HandleUiError(ex.Message, "CNC Control", logAsWarning: false);
            RefreshState();
        }
    }

    private void RunResponseAction(Func<string> action)
    {
        try
        {
            LastFeedback = action();
            RefreshState();
        }
        catch (Exception ex)
        {
            HandleUiError(ex.Message, "CNC Control", logAsWarning: false);
            RefreshState();
        }
    }

    private void Jog(string axis, decimal deltaMm)
    {
        try
        {
            LastFeedback = _cncControllerService.Jog(axis, deltaMm);
            RefreshState();
        }
        catch (Exception ex)
        {
            HandleUiError(ex.Message, "Jog blocked", logAsWarning: true);
            RefreshState();
        }
    }

    private void LoadProfileFields(CncMachineProfile profile)
    {
        _selectedProfile = profile;
        OnPropertyChanged(nameof(SelectedProfile));
        ProfileName = profile.ProfileName;
        ProfileDescription = profile.Description;
        ProfileNotes = profile.Notes;
        BaudRate = profile.BaudRate;
        SelectedDriverType = profile.DriverType;
        SelectedHomeOrigin = profile.HomeOriginConvention;
        SoftLimitsEnabled = profile.SoftLimitsEnabled;
        HomeXEnabled = profile.HomeXEnabled;
        HomeYEnabled = profile.HomeYEnabled;
        HomeZEnabled = profile.HomeZEnabled;
        SupportsXAxis = profile.SupportsXAxis;
        SupportsYAxis = profile.SupportsYAxis;
        SupportsZAxis = profile.SupportsZAxis;
        JogPresetsText = string.Join(", ", profile.JogPresets.Select(v => v.ToString("0.###")));
        XStepsPerMm = profile.XStepsPerMm;
        YStepsPerMm = profile.YStepsPerMm;
        ZStepsPerMm = profile.ZStepsPerMm;
        XMinMm = profile.XMinMm;
        XLimitMm = profile.XLimitMm;
        YMinMm = profile.YMinMm;
        YLimitMm = profile.YLimitMm;
        ZMinMm = profile.ZMinMm;
        ZLimitMm = profile.ZLimitMm;

        ReloadJogPresets(profile.JogPresets);
        OnPropertyChanged(nameof(ActiveProfileName));
        OnPropertyChanged(nameof(ActiveProfileDriver));
        OnPropertyChanged(nameof(IsSimulationMode));
        OnPropertyChanged(nameof(IsHardwarePortRequired));
        OnPropertyChanged(nameof(RuntimeModeText));
        RefreshProfilePermissionState();
        RefreshActiveMachineContext();
        SyncWorkflowContext();
        RefreshPorts();
        CommandManager.InvalidateRequerySuggested();
    }

    private void RefreshProfilePermissionState()
    {
        OnPropertyChanged(nameof(IsSelectedProfileBuiltIn));
        OnPropertyChanged(nameof(CanEditSelectedProfile));
        OnPropertyChanged(nameof(CanSaveSelectedProfile));
        OnPropertyChanged(nameof(CanDeleteSelectedProfile));
        OnPropertyChanged(nameof(SelectedProfileKindDisplay));
        OnPropertyChanged(nameof(ProfilePermissionHint));
    }

    private string? ValidateDefinitionProfileRules(CncMachineProfile selectedProfile, IReadOnlyList<decimal> parsedJogPresets)
    {
        var definition = selectedProfile.DefinitionSnapshot;
        if (definition == null)
            return null;

        var allowed = definition.ProfileRules.AllowedOverrides;
        if (SelectedDriverType != selectedProfile.DriverType && !allowed.Contains(OverrideField.DriverType))
            return "This machine definition does not allow driver type overrides. Duplicate/select a definition that allows DriverType overrides.";

        if (BaudRate != selectedProfile.BaudRate && !allowed.Contains(OverrideField.BaudRate))
            return "This machine definition does not allow baud-rate overrides.";

        if ((XStepsPerMm != selectedProfile.XStepsPerMm || YStepsPerMm != selectedProfile.YStepsPerMm || ZStepsPerMm != selectedProfile.ZStepsPerMm) && !allowed.Contains(OverrideField.StepsPerMm))
            return "This machine definition does not allow steps/mm overrides.";

        if (!parsedJogPresets.SequenceEqual(selectedProfile.JogPresets.OrderBy(v => v)) && !allowed.Contains(OverrideField.JogPresets))
            return "This machine definition does not allow jog preset overrides.";

        if (!string.Equals(ProfileNotes.Trim(), selectedProfile.Notes, StringComparison.Ordinal) && !allowed.Contains(OverrideField.Notes))
            return "This machine definition does not allow notes overrides.";

        if (XMinMm != selectedProfile.XMinMm || XLimitMm != selectedProfile.XLimitMm || YMinMm != selectedProfile.YMinMm || YLimitMm != selectedProfile.YLimitMm || ZMinMm != selectedProfile.ZMinMm || ZLimitMm != selectedProfile.ZLimitMm)
            return "Machine travel limits come from the backend MachineDefinition and cannot be edited in this runtime profile.";

        foreach (var constraint in definition.ProfileRules.OverrideConstraints)
        {
            if (constraint.ConstraintType != ConstraintType.Range)
                continue;

            if (constraint.Field == OverrideField.BaudRate && (BaudRate < constraint.MinValue || BaudRate > constraint.MaxValue))
                return $"Baud rate must stay within [{constraint.MinValue:0.###}, {constraint.MaxValue:0.###}] for this machine definition.";
        }

        return null;
    }

    private void SaveConfig()
    {
        if (SelectedProfile?.IsBuiltIn == true)
        {
            HandleUiError("System CNC profiles are read-only. Duplicate the profile to create an editable copy.", "Save CNC Configuration", logAsWarning: true);
            return;
        }

        if (XMinMm >= XLimitMm || YMinMm >= YLimitMm || ZMinMm >= ZLimitMm)
        {
            HandleUiError("Minimum bounds must be less than maximum bounds.", "Save CNC Configuration", logAsWarning: true);
            return;
        }

        if (BaudRate <= 0 || XStepsPerMm <= 0 || YStepsPerMm <= 0 || ZStepsPerMm <= 0)
        {
            HandleUiError("Baud rate and steps/mm values must be greater than zero.", "Save CNC Configuration", logAsWarning: true);
            return;
        }

        if (!SupportsXAxis && !SupportsYAxis && !SupportsZAxis)
        {
            HandleUiError("At least one machine axis must remain enabled.", "Save CNC Configuration", logAsWarning: true);
            return;
        }

        if (SelectedProfile == null)
        {
            HandleUiError("Select a machine profile first.", "Save CNC Configuration", logAsWarning: true);
            return;
        }

        var parsedJogPresets = ParseJogPresets();
        if (parsedJogPresets.Count == 0)
        {
            HandleUiError("Add at least one valid jog preset.", "Save CNC Configuration", logAsWarning: true);
            return;
        }

        var profileRulesError = ValidateDefinitionProfileRules(SelectedProfile, parsedJogPresets);
        if (profileRulesError != null)
        {
            HandleUiError(profileRulesError, "Save CNC Configuration", logAsWarning: true);
            return;
        }

        var updatedProfile = new CncMachineProfile
        {
            ProfileId = SelectedProfile.ProfileId,
            ProfileName = string.IsNullOrWhiteSpace(ProfileName) ? "Unnamed CNC Profile" : ProfileName.Trim(),
            MachineType = SelectedProfile.MachineType,
            Description = ProfileDescription.Trim(),
            Notes = ProfileNotes.Trim(),
            IsDefault = SelectedProfile.IsDefault,
            IsBuiltIn = SelectedProfile.IsBuiltIn,
            MachineDefinitionId = SelectedProfile.MachineDefinitionId,
            MachineDefinitionVersion = SelectedProfile.MachineDefinitionVersion,
            DefinitionSnapshot = SelectedProfile.DefinitionSnapshot,
            CompatibilityState = SelectedProfile.CompatibilityState,
            DriverType = SelectedDriverType,
            BaudRate = BaudRate,
            XStepsPerMm = XStepsPerMm,
            YStepsPerMm = YStepsPerMm,
            ZStepsPerMm = ZStepsPerMm,
            XMinMm = XMinMm,
            XLimitMm = XLimitMm,
            YMinMm = YMinMm,
            YLimitMm = YLimitMm,
            ZMinMm = ZMinMm,
            ZLimitMm = ZLimitMm,
            HomeOriginConvention = SelectedHomeOrigin,
            HomeXEnabled = HomeXEnabled,
            HomeYEnabled = HomeYEnabled,
            HomeZEnabled = HomeZEnabled,
            SupportsXAxis = SupportsXAxis,
            SupportsYAxis = SupportsYAxis,
            SupportsZAxis = SupportsZAxis,
            SoftLimitsEnabled = SoftLimitsEnabled,
            JogPresets = parsedJogPresets,
            VisualizationWidthMm = XLimitMm - XMinMm,
            VisualizationHeightMm = YLimitMm - YMinMm,
            VisualizationDepthMm = ZLimitMm - ZMinMm
        };

        _cncProfileService.SaveProfile(updatedProfile);
        if (_cncProfileService.ActiveProfile.ProfileId == updatedProfile.ProfileId)
            _cncControllerService.UpdateConfig(updatedProfile.ToMachineConfig());

        RevalidateLoadedProgram();
        ReloadJogPresets(updatedProfile.JogPresets);
        AddDiagnostic("Info", $"CNC profile '{updatedProfile.ProfileName}' saved.");
        LastFeedback = $"CNC profile '{updatedProfile.ProfileName}' saved.";
    }

    private void LoadGcodeFile()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "G-code files (*.gcode;*.nc;*.txt)|*.gcode;*.nc;*.txt|All files (*.*)|*.*",
            Title = "Load G-code"
        };

        if (dialog.ShowDialog(Application.Current.MainWindow) != true)
            return;

        try
        {
            ClearDiagnostics();
            LastFeedback = "Parsing G-code file...";
            AddDiagnostic("Info", $"Loading file: {dialog.FileName}");
            var parsed = _gcodeParserService.ParseFile(dialog.FileName);
            LoadedProgram = parsed;
            _jobPlacement = new CncJobPlacement();
            PlacementOffsetX = 0m;
            PlacementOffsetY = 0m;
            _jobSessionService.LoadJob(parsed, ActiveProfileName, RuntimeModeText, ActiveJob?.Title, ActiveJob?.JobReference);
            RevalidateLoadedProgram();
            LastFeedback = $"Loaded {LoadedProgram.FileName}.";
            AddDiagnostic("Info", $"Parse completed with {LoadedProgram.Motions.Count} motion line(s).");
            AddDiagnostics(LoadedProgram.Messages, defaultSeverity: "Warning");
        }
        catch (Exception ex)
        {
            HandleUiError(ex.Message, "Load G-code", logAsWarning: false);
        }
    }

    private void ClearLoadedProgram()
    {
        LoadedProgram = null;
        _jobSessionService.ClearLoadedJob();
        _previewPlaybackService.Stop();
        _previewPlaybackService.Load(Array.Empty<GcodeMotionCommand>());
        _placedMotions.Clear();
        _frameBounds = new CncFrameBounds();
        _jobPlacement = new CncJobPlacement();
        PlacementOffsetX = 0m;
        PlacementOffsetY = 0m;
        PlacementStatus = "Placement offsets are zeroed.";
        ClearDiagnostics();
        _executionQueueService.Load(Array.Empty<GcodeMotionCommand>());
        LastFeedback = "Loaded G-code cleared.";
        AddDiagnostic("Info", "Loaded G-code cleared.");
        RefreshWorkflowState();
    }

    private async Task StartProgramAsync()
    {
        try
        {
            if (!IsConnected)
                throw new InvalidOperationException("Connect the CNC machine before starting a G-code job.");

            if (!MotorsEnabled)
                throw new InvalidOperationException("Enable motors before starting a G-code job.");

            if (LoadedProgram == null)
                throw new InvalidOperationException("Load a G-code file before starting execution.");

            RevalidateLoadedProgram();
            if (ErrorCount > 0 || _placedMotions.Any(m => !m.IsValid))
                throw new InvalidOperationException("Critical validation issues are blocking execution.");

            _executionQueueService.Load(_placedMotions.ToList());
            _jobSessionService.StartSession(TotalMotionCount);
            LastFeedback = "Execution started.";
            AddDiagnostic("Info", "Execution started.");
            _cncControllerService.RefreshStatus();
            await _executionQueueService.StartAsync(_cncControllerService);
            if (_executionQueueService.ExecutionState == CncExecutionState.Completed)
            {
                LastFeedback = "Execution completed.";
                AddDiagnostic("Info", "Execution completed.");
            }
        }
        catch (Exception ex)
        {
            HandleUiError(ex.Message, "Start G-code Execution", logAsWarning: false);
        }
        finally
        {
            RefreshState();
        }
    }

    private void PauseProgram()
    {
        _executionQueueService.Pause();
        _jobSessionService.PauseSession("Operator paused the CNC job session.");
        LastFeedback = "Execution paused.";
        AddDiagnostic("Info", "Execution paused.");
        RefreshState();
    }

    private async Task ResumeProgramAsync()
    {
        try
        {
            _jobSessionService.ResumeSession("Operator resumed the CNC job session.");
            LastFeedback = "Execution resumed.";
            AddDiagnostic("Info", "Execution resumed.");
            await _executionQueueService.ResumeAsync(_cncControllerService);
            if (_executionQueueService.ExecutionState == CncExecutionState.Completed)
            {
                LastFeedback = "Execution completed.";
                AddDiagnostic("Info", "Execution completed.");
            }
        }
        catch (Exception ex)
        {
            HandleUiError(ex.Message, "Resume G-code Execution", logAsWarning: false);
        }
        finally
        {
            RefreshState();
        }
    }

    private async Task StopExecutionAsync()
    {
        try
        {
            await _executionQueueService.StopAsync(_cncControllerService);
            _jobSessionService.StopSession(CompletedMotionCount, TotalMotionCount, MachineX, MachineY, MachineZ, "Operator stopped the CNC job session.");
            LastFeedback = "Execution stopped.";
            AddDiagnostic("Warning", "Execution stopped by operator.");
        }
        catch (Exception ex)
        {
            HandleUiError(ex.Message, "Stop G-code Execution", logAsWarning: false);
        }
        finally
        {
            RefreshState();
        }
    }

    private void ApplySelectedProfile()
    {
        if (SelectedProfile == null)
            return;

        if (_executionQueueService.ExecutionState is CncExecutionState.Running or CncExecutionState.Paused)
        {
            HandleUiError("Stop the active CNC execution before switching machine profiles.", "Switch CNC Profile", logAsWarning: true);
            return;
        }

        if (IsConnected)
        {
            var result = MessageBox.Show(
                Application.Current.MainWindow,
                "Switching the active profile will disconnect the current CNC session. Continue?",
                "Switch CNC Profile",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;
        }

        if (CanEditSelectedProfile)
            SaveConfig();
        _cncProfileService.SetActiveProfile(SelectedProfile.ProfileId);
        AddDiagnostic("Info", $"Active profile changed to '{_cncProfileService.ActiveProfile.ProfileName}'.");
        LastFeedback = $"Active profile set to '{_cncProfileService.ActiveProfile.ProfileName}'.";
    }

    private void DuplicateSelectedProfile()
    {
        if (SelectedProfile == null)
            return;

        if (CanEditSelectedProfile)
            SaveConfig();
        var duplicate = _cncProfileService.DuplicateProfile(SelectedProfile.ProfileId);
        SelectedProfile = duplicate;
        AddDiagnostic("Info", $"Duplicated profile '{duplicate.ProfileName}'.");
        LastFeedback = $"Duplicated profile '{duplicate.ProfileName}'.";
    }

    private void DeleteSelectedProfile()
    {
        if (SelectedProfile == null)
            return;

        if (SelectedProfile.IsBuiltIn || SelectedProfile.IsDefault)
        {
            HandleUiError("System CNC profiles cannot be deleted. Duplicate the profile to create an editable copy.", "Delete CNC Profile", logAsWarning: true);
            return;
        }

        var result = MessageBox.Show(
            Application.Current.MainWindow,
            $"Delete machine profile '{SelectedProfile.ProfileName}'?",
            "Delete CNC Profile",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
            return;

        var deletedProfileName = SelectedProfile.ProfileName;
        if (!_cncProfileService.DeleteProfile(SelectedProfile.ProfileId))
        {
            HandleUiError("Unable to delete the selected machine profile.", "Delete CNC Profile", logAsWarning: true);
            return;
        }

        LoadProfileFields(_cncProfileService.ActiveProfile);
        AddDiagnostic("Info", $"Deleted profile '{deletedProfileName}'.");
        LastFeedback = $"Deleted profile '{deletedProfileName}'.";
    }

    private void RestoreDefaultProfiles()
    {
        if (_executionQueueService.ExecutionState is CncExecutionState.Running or CncExecutionState.Paused)
        {
            HandleUiError("Stop CNC execution before restoring default profiles.", "Restore CNC Profiles", logAsWarning: true);
            return;
        }

        var result = MessageBox.Show(
            Application.Current.MainWindow,
            "Restore the default CNC profile set? Custom profile changes will be removed.",
            "Restore CNC Profiles",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
            return;

        _cncProfileService.RestoreDefaultProfiles();
        LoadProfileFields(_cncProfileService.ActiveProfile);
        RevalidateLoadedProgram();
        AddDiagnostic("Info", "Restored default CNC profiles.");
        LastFeedback = "Restored default CNC profiles.";
    }

    private async Task SyncMachineCatalogAsync()
    {
        try
        {
            var previousCategoryId = SelectedMachineCategory?.Id;
            var previousFamilyId = SelectedMachineFamily?.Id;
            var previousDefinitionId = SelectedMachineDefinitionSummary?.Id;
            var previousRuntimeProfileId = SelectedRuntimeProfile?.RuntimeProfileId;

            MachinePlatformStatus = "Syncing machine catalog from backend...";
            await _machineCatalogService.RefreshAsync();
            var createdDefaults = _runtimeProfileService.EnsureSystemProfilesForDefinitions(_machineCatalogService.CachedDefinitions.ToList());
            _runtimeProfileService.RecomputeCompatibility(_machineCatalogService.CachedDefinitions.ToList());
            MachinePlatformStatus = createdDefaults > 0
                ? $"{_machineCatalogService.LastSyncStatus} Default profiles are ready in the background."
                : $"{_machineCatalogService.LastSyncStatus} Choose a machine to continue.";
            OnPropertyChanged(nameof(MachineCatalogSyncStatus));
            OnPropertyChanged(nameof(MachineCategories));
            OnPropertyChanged(nameof(MachineFamilies));
            OnPropertyChanged(nameof(MachineDefinitionSummaries));
            RefreshWizardCards();
            RefreshWizardStepState();
            RestoreMachineSelectionAfterSync(previousCategoryId, previousFamilyId, previousDefinitionId, previousRuntimeProfileId);
            RefreshRuntimeProfiles();
            AddDiagnostic("Info", MachinePlatformStatus);
        }
        catch (Exception ex)
        {
            HandleUiError(ex.Message, "Machine Catalog Sync", logAsWarning: true);
        }
    }

    private async Task CreateRuntimeProfileFromSelectionAsync()
    {
        if (SelectedMachineDefinitionSummary == null)
            return;

        try
        {
            var definition = await _machineCatalogService.GetDefinitionAsync(SelectedMachineDefinitionSummary.Id);
            if (definition == null)
            {
                HandleUiError("Machine definition is not available from backend or local cache.", "Create Runtime Profile", logAsWarning: true);
                return;
            }

            var profileName = string.IsNullOrWhiteSpace(NewRuntimeProfileName) ? $"{definition.DisplayNameEn} Custom" : NewRuntimeProfileName.Trim();
            var driver = definition.RuntimeBinding.DefaultDriverType;
            var runtimeProfile = _runtimeProfileService.CreateFromDefinition(definition, profileName, RuntimeProfileType.User, driver);
            var cncProfile = _cncProfileService.CreateProfileFromMachineDefinition(definition, profileName, driver, RuntimeProfileType.User);
            SelectedRuntimeProfile = runtimeProfile;
            SelectedProfile = cncProfile;
            MachinePlatformStatus = $"Created custom runtime profile '{profileName}' from {definition.DisplayNameEn} v{definition.Version}.";
            AddDiagnostic("Info", MachinePlatformStatus);
        }
        catch (Exception ex)
        {
            HandleUiError(ex.Message, "Create Runtime Profile", logAsWarning: false);
        }
    }

    private void OpenMachineWizard()
    {
        _wizardSelectedFamilyCard = null;
        _wizardSelectedMachineCard = null;
        _wizardSelectedModeCard = null;
        WizardSelectedSetupMode = null;
        RefreshWizardCards();
        WizardStep = MachineWizardStep.Catalog;
        IsMachineWizardOpen = true;
        MachinePlatformStatus = "Choose a machine definition to add or switch the CNC workspace.";
    }

    private void CloseMachineWizard()
    {
        IsMachineWizardOpen = false;
    }

    private async Task ConfirmMachineWizardAsync()
    {
        if (WizardSelectedMachineDefinition == null)
            return;

        try
        {
            var definition = await _machineCatalogService.GetDefinitionAsync(WizardSelectedMachineDefinition.Id);
            if (definition == null)
            {
                HandleUiError("Machine definition is not available from backend or local cache.", "Add Machine", logAsWarning: true);
                return;
            }

            _runtimeProfileService.EnsureSystemProfilesForDefinitions(new[] { definition });
            _runtimeProfileService.RecomputeCompatibility(_machineCatalogService.CachedDefinitions.ToList());

            var profile = _runtimeProfileService.Profiles
                .Where(p => p.MachineDefinitionId == definition.Id)
                .Where(p => string.Equals(p.MachineDefinitionVersion, definition.Version, StringComparison.OrdinalIgnoreCase))
                .OrderBy(p => p.ProfileType == RuntimeProfileType.System ? 0 : 1)
                .ThenBy(p => p.ProfileName)
                .FirstOrDefault();

            if (profile == null)
            {
                HandleUiError("No runtime profile could be created for the selected machine.", "Add Machine", logAsWarning: true);
                return;
            }

            SelectedMachineCategory = _machineCatalogService.Categories.FirstOrDefault(c => c.Id == definition.CategoryId);
            SelectedMachineFamily = _machineCatalogService.Families.FirstOrDefault(f => f.Id == definition.FamilyId);
            SelectedMachineDefinitionSummary = _machineCatalogService.DefinitionSummaries.FirstOrDefault(d => d.Id == definition.Id) ?? WizardSelectedMachineDefinition;
            SelectedRuntimeProfile = profile;

            ActivateRuntimeProfile(profile);
            IsMachineWizardOpen = false;
            MachinePlatformStatus = $"Using {definition.DisplayNameEn} with runtime profile '{profile.ProfileName}'.";
            AddDiagnostic("Info", MachinePlatformStatus);
        }
        catch (Exception ex)
        {
            HandleUiError(ex.Message, "Add Machine", logAsWarning: true);
        }
    }

    private async Task MoveWizardNextAsync()
    {
        if (!CanMoveWizardNext)
            return;

        if (WizardStep == MachineWizardStep.Confirm)
        {
            await ConfirmMachineWizardAsync();
            return;
        }

        WizardStep = (MachineWizardStep)((int)WizardStep + 1);
    }

    private void MoveWizardBack()
    {
        if (!CanMoveWizardBack)
            return;

        WizardStep = (MachineWizardStep)((int)WizardStep - 1);
    }

    private void SelectWizardFamily(object? parameter)
    {
        if (parameter is not MachineWizardCard card || card.Family == null)
            return;

        foreach (var item in WizardFamilyCards)
            item.IsSelected = ReferenceEquals(item, card);

        _wizardSelectedFamilyCard = card;
        _wizardSelectedMachineCard = null;
        _wizardSelectedModeCard = null;
        WizardSelectedSetupMode = null;
        BuildWizardMachineCards();
        BuildWizardModeCards();
        RefreshWizardStepState();
    }

    private void SelectWizardMachine(object? parameter)
    {
        if (parameter is not MachineWizardCard card || card.MachineSummary == null)
            return;

        foreach (var item in WizardMachineCards)
            item.IsSelected = ReferenceEquals(item, card);

        _wizardSelectedMachineCard = card;
        _wizardSelectedModeCard = null;
        BuildWizardModeCards();
        RefreshWizardStepState();
    }

    private void SelectWizardMode(object? parameter)
    {
        if (parameter is not MachineWizardCard card || card.SetupMode == null)
            return;

        foreach (var item in WizardModeCards)
            item.IsSelected = ReferenceEquals(item, card);

        _wizardSelectedModeCard = card;
        WizardSelectedSetupMode = card.SetupMode;
        RefreshWizardStepState();
    }

    private void ActivateSelectedRuntimeProfile()
    {
        if (SelectedRuntimeProfile == null)
            return;

        ActivateRuntimeProfile(SelectedRuntimeProfile);
    }

    private void ConfirmAndActivateSelectedMachine()
    {
        if (SelectedMachineDefinitionSummary == null || SelectedRuntimeProfile == null)
            return;

        var result = MessageBox.Show(
            Application.Current.MainWindow,
            $"Use this machine for the CNC workspace?\n\nMachine: {SelectedMachineDefinitionSummary.DisplayNameEn}\nFamily: {SelectedMachineFamily?.DisplayNameEn ?? "Not selected"}\nProfile: {SelectedRuntimeProfile.ProfileName}\n\nThis will switch the active runtime profile.",
            "Use Machine",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes)
            return;

        ActivateRuntimeProfile(SelectedRuntimeProfile);
    }

    private void ActivateRuntimeProfile(RuntimeProfile runtimeProfile)
    {
        if (!_runtimeProfileService.SetActiveProfile(runtimeProfile.RuntimeProfileId))
        {
            HandleUiError("Runtime profile cannot be activated because its machine definition is missing or incompatible.", "Activate Runtime Profile", logAsWarning: true);
            return;
        }

        ApplyRuntimeProfileToCncRuntime(runtimeProfile);
        RefreshActiveMachineContext();
        MachinePlatformStatus = $"Activated runtime profile '{runtimeProfile.ProfileName}'.";
        AddDiagnostic("Info", MachinePlatformStatus);
    }

    private void DuplicateSelectedRuntimeProfile()
    {
        if (SelectedRuntimeProfile == null)
            return;

        try
        {
            SelectedRuntimeProfile = _runtimeProfileService.DuplicateProfile(SelectedRuntimeProfile.RuntimeProfileId);
            MachinePlatformStatus = $"Duplicated runtime profile '{SelectedRuntimeProfile.ProfileName}'.";
            AddDiagnostic("Info", MachinePlatformStatus);
        }
        catch (Exception ex)
        {
            HandleUiError(ex.Message, "Duplicate Runtime Profile", logAsWarning: true);
        }
    }

    private void DeleteSelectedRuntimeProfile()
    {
        if (SelectedRuntimeProfile == null)
            return;

        if (!_runtimeProfileService.DeleteProfile(SelectedRuntimeProfile.RuntimeProfileId))
        {
            HandleUiError("Only user runtime profiles can be deleted.", "Delete Runtime Profile", logAsWarning: true);
            return;
        }

        SelectedRuntimeProfile = _runtimeProfileService.ActiveProfile;
        MachinePlatformStatus = "Runtime profile deleted.";
        AddDiagnostic("Info", MachinePlatformStatus);
    }

    private void RefreshRuntimeProfiles()
    {
        OnPropertyChanged(nameof(RuntimeProfiles));
        OnPropertyChanged(nameof(SelectedRuntimeProfile));
        OnPropertyChanged(nameof(HasSelectedRuntimeProfile));
        OnPropertyChanged(nameof(CanDeleteSelectedRuntimeProfile));
        CommandManager.InvalidateRequerySuggested();
    }

    private void RestoreMachineSelectionAfterSync(Guid? categoryId, Guid? familyId, Guid? definitionId, string? runtimeProfileId)
    {
        if (definitionId == null)
        {
            _selectedMachineCategory = null;
            _selectedMachineFamily = null;
            _selectedMachineDefinitionSummary = null;
            _selectedRuntimeProfile = null;
            OnPropertyChanged(nameof(SelectedMachineCategory));
            OnPropertyChanged(nameof(SelectedMachineFamily));
            OnPropertyChanged(nameof(SelectedMachineDefinitionSummary));
            OnPropertyChanged(nameof(SelectedRuntimeProfile));
            OnPropertyChanged(nameof(HasSelectedMachineDefinition));
            OnPropertyChanged(nameof(HasSelectedRuntimeProfile));
            OnPropertyChanged(nameof(MachineFamilies));
            OnPropertyChanged(nameof(MachineDefinitionSummaries));
            return;
        }

        var definition = _machineCatalogService.DefinitionSummaries.FirstOrDefault(d => d.Id == definitionId.Value);
        if (definition == null)
        {
            _selectedMachineCategory = null;
            _selectedMachineFamily = null;
            _selectedMachineDefinitionSummary = null;
            _selectedRuntimeProfile = null;
            OnPropertyChanged(nameof(SelectedMachineCategory));
            OnPropertyChanged(nameof(SelectedMachineFamily));
            OnPropertyChanged(nameof(SelectedMachineDefinitionSummary));
            OnPropertyChanged(nameof(SelectedRuntimeProfile));
            OnPropertyChanged(nameof(HasSelectedMachineDefinition));
            OnPropertyChanged(nameof(HasSelectedRuntimeProfile));
            OnPropertyChanged(nameof(MachineFamilies));
            OnPropertyChanged(nameof(MachineDefinitionSummaries));
            return;
        }

        _selectedMachineCategory = _machineCatalogService.Categories.FirstOrDefault(c => c.Id == (categoryId ?? definition.CategoryId));
        _selectedMachineFamily = _machineCatalogService.Families.FirstOrDefault(f => f.Id == (familyId ?? definition.FamilyId));
        _selectedMachineDefinitionSummary = definition;
        _newRuntimeProfileName = $"{definition.DisplayNameEn} Custom";
        _selectedRuntimeProfile = _runtimeProfileService.Profiles.FirstOrDefault(profile => profile.RuntimeProfileId == runtimeProfileId)
                                  ?? _runtimeProfileService.Profiles
                                      .Where(profile => profile.MachineDefinitionId == definition.Id)
                                      .OrderBy(profile => profile.ProfileType == RuntimeProfileType.System ? 0 : 1)
                                      .ThenBy(profile => profile.ProfileName)
                                      .FirstOrDefault();
        OnPropertyChanged(nameof(SelectedMachineCategory));
        OnPropertyChanged(nameof(SelectedMachineFamily));
        OnPropertyChanged(nameof(SelectedMachineDefinitionSummary));
        OnPropertyChanged(nameof(SelectedRuntimeProfile));
        OnPropertyChanged(nameof(NewRuntimeProfileName));
        OnPropertyChanged(nameof(HasSelectedMachineDefinition));
        OnPropertyChanged(nameof(HasSelectedRuntimeProfile));
        OnPropertyChanged(nameof(MachineFamilies));
        OnPropertyChanged(nameof(MachineDefinitionSummaries));
    }

    private void SelectPreferredRuntimeProfileForSelectedMachine()
    {
        if (SelectedMachineDefinitionSummary == null)
        {
            SelectedRuntimeProfile = null;
            return;
        }

        var profiles = _runtimeProfileService.Profiles
            .Where(profile => profile.MachineDefinitionId == SelectedMachineDefinitionSummary.Id)
            .ToList();

        SelectedRuntimeProfile =
            profiles.FirstOrDefault(profile => profile.ProfileType == RuntimeProfileType.System)
            ?? profiles.FirstOrDefault(profile => profile.IsActive)
            ?? profiles.FirstOrDefault();
    }

    private void RefreshWizardCards()
    {
        BuildWizardSteps();
        BuildWizardFamilyCards();
        BuildWizardMachineCards();
        BuildWizardModeCards();
        OnPropertyChanged(nameof(WizardSyncDisplay));
        OnPropertyChanged(nameof(WizardConnectivityText));
        OnPropertyChanged(nameof(HasWizardMachines));
    }

    private void BuildWizardSteps()
    {
        if (WizardSteps.Count == 0)
        {
            WizardSteps.Add(new MachineWizardStepItem(MachineWizardStep.Catalog, "1 Catalog"));
            WizardSteps.Add(new MachineWizardStepItem(MachineWizardStep.Family, "2 Family"));
            WizardSteps.Add(new MachineWizardStepItem(MachineWizardStep.Machine, "3 Machine"));
            WizardSteps.Add(new MachineWizardStepItem(MachineWizardStep.Mode, "4 Mode"));
            WizardSteps.Add(new MachineWizardStepItem(MachineWizardStep.Confirm, "5 Confirm"));
        }

        foreach (var step in WizardSteps)
        {
            step.IsCurrent = step.Step == WizardStep;
            step.IsCompleted = step.Step < WizardStep;
        }
    }

    private void BuildWizardFamilyCards()
    {
        WizardFamilyCards.Clear();
        foreach (var family in _machineCatalogService.Families.OrderBy(f => f.SortOrder).ThenBy(f => f.DisplayNameEn))
        {
            WizardFamilyCards.Add(new MachineWizardCard
            {
                Title = family.DisplayNameEn,
                Subtitle = string.IsNullOrWhiteSpace(family.DescriptionEn) ? family.Manufacturer : family.DescriptionEn,
                IconText = "▣",
                Family = family,
                IsSelected = _wizardSelectedFamilyCard?.Family?.Id == family.Id
            });
        }
    }

    private void BuildWizardMachineCards()
    {
        WizardMachineCards.Clear();
        if (WizardSelectedMachineFamily == null)
            return;

        var machines = _machineCatalogService.DefinitionSummaries
            .Where(d => d.FamilyId == WizardSelectedMachineFamily.Id)
            .OrderBy(d => d.SortOrder)
            .ThenBy(d => d.DisplayNameEn);

        foreach (var summary in machines)
        {
            var definition = _machineCatalogService.GetCachedDefinition(summary.Id, summary.Version);
            var workspace = definition?.Workspace.WorkAreaMm;
            var axes = definition?.AxisConfig.SupportedAxes.Count > 0
                ? string.Join(" ", definition.AxisConfig.SupportedAxes)
                : "X Y Z";

            var tags = new ObservableCollection<string>();
            tags.Add("CNC");
            if (definition?.FileSupport.GcodeDialect != null || !string.IsNullOrWhiteSpace(summary.DefaultDriverType))
                tags.Add("G-code");
            tags.Add($"{axes.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length}-axis");

            WizardMachineCards.Add(new MachineWizardCard
            {
                Title = summary.DisplayNameEn,
                Subtitle = FirstNonEmpty(summary.DescriptionEn, definition?.DescriptionEn, summary.Manufacturer, "Machine definition"),
                VersionText = $"v{summary.Version}",
                WorkspaceText = workspace != null ? $"{workspace.Width:0.#} x {workspace.Depth:0.#} x {workspace.Height:0.#} mm" : "Workspace from definition",
                AxesText = axes,
                ImageUrl = FirstNonEmpty(summary.ThumbnailUrl, summary.ImageUrl, summary.ImageSource, definition?.ThumbnailUrl, definition?.ImageUrl, definition?.ImageSource),
                MachineSummary = summary,
                MachineDefinition = definition,
                Tags = tags,
                IsSelected = _wizardSelectedMachineCard?.MachineSummary?.Id == summary.Id
            });
        }
    }

    private void BuildWizardModeCards()
    {
        WizardModeCards.Clear();
        foreach (var mode in GetWizardSetupModes())
        {
            WizardModeCards.Add(new MachineWizardCard
            {
                Title = FormatSetupMode(mode),
                Subtitle = mode switch
                {
                    SetupMode.RealOnly => "Connect to physical hardware.",
                    SetupMode.SimulationOnly => "Run a virtual machine.",
                    SetupMode.RealAndSimulation => "Use real hardware with simulation fallback.",
                    _ => "Machine setup mode."
                },
                IconText = mode switch
                {
                    SetupMode.RealOnly => "⌁",
                    SetupMode.SimulationOnly => "◇",
                    SetupMode.RealAndSimulation => "◈",
                    _ => "○"
                },
                SetupMode = mode,
                IsSelected = _wizardSelectedModeCard?.SetupMode == mode
            });
        }

        if (WizardModeCards.Count == 1)
            SelectWizardMode(WizardModeCards[0]);
    }

    private void RefreshWizardStepState()
    {
        BuildWizardSteps();
        OnPropertyChanged(nameof(IsWizardCatalogStep));
        OnPropertyChanged(nameof(IsWizardFamilyStep));
        OnPropertyChanged(nameof(IsWizardMachineStep));
        OnPropertyChanged(nameof(IsWizardModeStep));
        OnPropertyChanged(nameof(IsWizardConfirmStep));
        OnPropertyChanged(nameof(CanMoveWizardBack));
        OnPropertyChanged(nameof(CanMoveWizardNext));
        OnPropertyChanged(nameof(WizardNextButtonText));
        OnPropertyChanged(nameof(WizardSelectedMachineFamily));
        OnPropertyChanged(nameof(WizardSelectedMachineDefinition));
        OnPropertyChanged(nameof(WizardSelectedSetupMode));
        CommandManager.InvalidateRequerySuggested();
    }

    private IEnumerable<SetupMode> GetWizardSetupModes()
    {
        if (WizardSelectedMachineDefinition?.SupportedSetupModes is { Count: > 0 } modes)
        {
            foreach (var mode in modes)
            {
                if (Enum.TryParse<SetupMode>(mode, ignoreCase: true, out var parsed))
                    yield return parsed;
            }

            yield break;
        }

        yield return SetupMode.RealAndSimulation;
    }

    private static string FirstNonEmpty(params string?[] values)
        => values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty;

    private static string FormatSetupMode(SetupMode mode) => mode switch
    {
        SetupMode.RealOnly => "Real Machine",
        SetupMode.SimulationOnly => "Simulation",
        SetupMode.RealAndSimulation => "Real + Simulation",
        _ => mode.ToString()
    };

    private void RefreshActiveMachineContext()
    {
        var runtimeProfile = _runtimeProfileService.ActiveProfile;
        if (runtimeProfile == null && _cncProfileService.ActiveProfile.DefinitionSnapshot != null)
        {
            runtimeProfile = new RuntimeProfile
            {
                RuntimeProfileId = _cncProfileService.ActiveProfile.ProfileId,
                ProfileName = _cncProfileService.ActiveProfile.ProfileName,
                ProfileType = _cncProfileService.ActiveProfile.IsBuiltIn ? RuntimeProfileType.System : RuntimeProfileType.User,
                MachineDefinitionId = _cncProfileService.ActiveProfile.MachineDefinitionId ?? _cncProfileService.ActiveProfile.DefinitionSnapshot.Id,
                MachineDefinitionVersion = _cncProfileService.ActiveProfile.MachineDefinitionVersion ?? _cncProfileService.ActiveProfile.DefinitionSnapshot.Version,
                DefinitionSnapshot = _cncProfileService.ActiveProfile.DefinitionSnapshot,
                Overrides = new RuntimeProfileOverrides
                {
                    DriverType = _cncProfileService.ActiveProfile.DriverType == CncDriverType.Simulated ? DriverType.Simulated : DriverType.ArduinoSerial
                },
                CompatibilityState = _cncProfileService.ActiveProfile.CompatibilityState
            };
        }

        if (runtimeProfile == null)
        {
            var legacyDefinition = BuildDefinitionFromCncProfile(_cncProfileService.ActiveProfile);
            runtimeProfile = new RuntimeProfile
            {
                RuntimeProfileId = _cncProfileService.ActiveProfile.ProfileId,
                ProfileName = _cncProfileService.ActiveProfile.ProfileName,
                ProfileType = _cncProfileService.ActiveProfile.IsBuiltIn ? RuntimeProfileType.System : RuntimeProfileType.User,
                MachineDefinitionId = legacyDefinition.Id,
                MachineDefinitionVersion = legacyDefinition.Version,
                DefinitionSnapshot = legacyDefinition,
                Overrides = new RuntimeProfileOverrides
                {
                    DriverType = _cncProfileService.ActiveProfile.DriverType == CncDriverType.Simulated ? DriverType.Simulated : DriverType.ArduinoSerial
                }
            };
        }

        var liveDefinition = runtimeProfile.MachineDefinitionId is Guid id ? _machineCatalogService.GetCachedDefinition(id, runtimeProfile.MachineDefinitionVersion) : null;
        _activeMachineContextService.Resolve(runtimeProfile, liveDefinition);
        OnPropertyChanged(nameof(ActiveMachineContextText));
        OnPropertyChanged(nameof(EffectiveCapabilitiesSummary));
    }

    private void ApplyRuntimeProfileToCncRuntime(RuntimeProfile runtimeProfile)
    {
        var definition = _machineCatalogService.GetCachedDefinition(runtimeProfile.MachineDefinitionId, runtimeProfile.MachineDefinitionVersion)
                         ?? runtimeProfile.DefinitionSnapshot;
        if (definition == null)
            throw new InvalidOperationException("Runtime profile cannot be applied because its machine definition is missing.");

        var driver = runtimeProfile.Overrides.DriverType ?? definition.RuntimeBinding.DefaultDriverType;
        var existing = _cncProfileService.Profiles.FirstOrDefault(p =>
            p.MachineDefinitionId == definition.Id
            && string.Equals(p.MachineDefinitionVersion, definition.Version, StringComparison.OrdinalIgnoreCase)
            && string.Equals(p.ProfileName, runtimeProfile.ProfileName, StringComparison.OrdinalIgnoreCase));

        var cncProfile = existing ?? _cncProfileService.CreateProfileFromMachineDefinition(definition, runtimeProfile.ProfileName, driver, runtimeProfile.ProfileType);
        if (existing != null && existing.DriverType != (driver == DriverType.Simulated ? CncDriverType.Simulated : CncDriverType.ArduinoSerial) && existing.IsEditable)
        {
            existing.DriverType = driver == DriverType.Simulated ? CncDriverType.Simulated : CncDriverType.ArduinoSerial;
            _cncProfileService.SaveProfile(existing);
        }

        _cncProfileService.SetActiveProfile(cncProfile.ProfileId);
    }

    private void ReloadJogPresets(IEnumerable<decimal> presets)
    {
        JogStepPresets.Clear();
        foreach (var preset in presets.Distinct().OrderBy(p => p))
            JogStepPresets.Add(preset);

        SelectedJogStep = JogStepPresets.FirstOrDefault();
    }

    private List<decimal> ParseJogPresets()
    {
        return JogPresetsText
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(token => decimal.TryParse(token, out var value) ? value : -1m)
            .Where(value => value > 0m)
            .Distinct()
            .OrderBy(value => value)
            .ToList();
    }

    private void RevalidateLoadedProgram()
    {
        if (LoadedProgram == null)
        {
            _placedMotions.Clear();
            _frameBounds = new CncFrameBounds();
            _jobSessionService.UpdateReadiness(false, "Load a G-code job to prepare a CNC session.", "No CNC job is currently loaded.");
            return;
        }

        ClearDiagnostics();

        _placedMotions.Clear();
        _placedMotions.AddRange(_jobPlacementService.ApplyPlacement(LoadedProgram.Motions.ToList(), _jobPlacement));
        _frameBounds = _jobPlacementService.CalculateBounds(_placedMotions);

        foreach (var motion in _placedMotions)
        {
            motion.IsValid = true;
            motion.ValidationMessage = null;

            var boundsMessage = _cncControllerService.ValidateWorkPosition(motion.EndX, motion.EndY, motion.EndZ);
            if (boundsMessage != null)
            {
                motion.IsValid = false;
                motion.ValidationMessage = $"Line {motion.LineNumber}: {boundsMessage}";
            }

            if (!motion.IsValid && motion.ValidationMessage != null)
                AddDiagnostic("Error", motion.ValidationMessage);
        }

        AddDiagnostics(LoadedProgram.Messages, defaultSeverity: "Warning");
        _previewPlaybackService.Stop();
        _previewPlaybackService.Load(_placedMotions.ToList());
        _executionQueueService.Load(_placedMotions.ToList());
        OnPropertyChanged(nameof(MotionCommands));
        OnPropertyChanged(nameof(TotalMotionCount));
        OnPropertyChanged(nameof(DiagnosticsSummary));
        OnPropertyChanged(nameof(PlacementStatus));
        OnPropertyChanged(nameof(AppliedPlacementOffsetX));
        OnPropertyChanged(nameof(AppliedPlacementOffsetY));
        UpdateReadinessState();
        RefreshPreviewState();
    }

    private void ClearDiagnostics()
    {
        DiagnosticsMessages.Clear();
        OnPropertyChanged(nameof(HasDiagnostics));
        OnPropertyChanged(nameof(WarningCount));
        OnPropertyChanged(nameof(ErrorCount));
        OnPropertyChanged(nameof(DiagnosticsSummary));
        LastError = "No current alarms.";
    }

    private void AddDiagnostics(IEnumerable<string> messages, string defaultSeverity)
    {
        foreach (var message in messages)
            AddDiagnostic(defaultSeverity, message);
    }

    private void AddDiagnostic(string severity, string message)
    {
        var entry = $"[{severity}] {message}";
        if (DiagnosticsMessages.Contains(entry))
            return;

        DiagnosticsMessages.Add(entry);
        if (severity.Equals("Error", StringComparison.OrdinalIgnoreCase))
            LastError = message;

        OnPropertyChanged(nameof(HasDiagnostics));
        OnPropertyChanged(nameof(WarningCount));
        OnPropertyChanged(nameof(ErrorCount));
        OnPropertyChanged(nameof(DiagnosticsSummary));
    }

    private void SyncWorkflowContext()
    {
        _jobSessionService.UpdateRuntimeContext(ActiveProfileName, RuntimeModeText, ActiveJob?.Title, ActiveJob?.JobReference);
    }

    private void UpdateReadinessState()
    {
        if (!HasLoadedProgram)
        {
            _jobSessionService.UpdateReadiness(false, "Load a G-code job to prepare a CNC session.", "No CNC job is currently loaded.");
            return;
        }

        if (SelectedProfile == null)
        {
            _jobSessionService.UpdateReadiness(false, "Select a CNC profile before running.", "No active machine profile is selected.");
            return;
        }

        if (!IsConnected)
        {
            _jobSessionService.UpdateReadiness(false, "Machine is not connected.", "Connect the CNC machine before starting the job.");
            return;
        }

        if (!_cncControllerService.DeviceStatus.IsResponsive)
        {
            _jobSessionService.UpdateReadiness(false, "Waiting for machine handshake.", "The CNC device has not completed protocol handshake yet.");
            return;
        }

        if (!MotorsEnabled)
        {
            _jobSessionService.UpdateReadiness(false, "Machine is connected but motors are disabled.", "Enable motors before starting the CNC job.");
            return;
        }

        if (HasAlarm)
        {
            _jobSessionService.UpdateReadiness(false, "Machine alarm blocks execution.", _cncControllerService.LastFaultReason ?? "Clear the alarm or error state before starting.");
            return;
        }

        if (!_cncControllerService.HasValidMachineReference)
        {
            _jobSessionService.UpdateReadiness(false, "Machine reference has not been established yet.", "Home the machine before starting a production run.");
            return;
        }

        if (ErrorCount > 0 || _placedMotions.Any(m => !m.IsValid))
        {
            _jobSessionService.UpdateReadiness(false, "Loaded job has blocking validation issues.", "Resolve parser or bounds errors before starting the job.");
            return;
        }

        _jobSessionService.UpdateReadiness(true, "Ready to run. Machine, profile, and loaded job passed pre-run checks.", null);
    }

    private void ApplyPlacement()
    {
        if (LoadedProgram == null)
            return;

        var proposedPlacement = new CncJobPlacement
        {
            OffsetX = PlacementOffsetX,
            OffsetY = PlacementOffsetY
        };

        var validationMessage = _jobPlacementService.ValidatePlacement(LoadedProgram.Motions.ToList(), proposedPlacement, XMinMm, XLimitMm, YMinMm, YLimitMm);
        if (validationMessage != null)
        {
            HandleUiError(validationMessage, "Apply Job Placement", logAsWarning: true);
            return;
        }

        _jobPlacement = proposedPlacement;
        PlacementStatus = $"Placed at X {AppliedPlacementOffsetX:0.###} mm / Y {AppliedPlacementOffsetY:0.###} mm.";
        RevalidateLoadedProgram();
        LastFeedback = "Job placement updated.";
        AddDiagnostic("Info", $"Job placement updated to X {AppliedPlacementOffsetX:0.###} mm / Y {AppliedPlacementOffsetY:0.###} mm.");
    }

    private void ResetPlacement()
    {
        PlacementOffsetX = 0m;
        PlacementOffsetY = 0m;
        ApplyPlacement();
    }

    private void ApplyPlacementPreset(CncPlacementPreset preset)
    {
        if (LoadedProgram == null)
            return;

        var placement = _jobPlacementService.CreatePresetPlacement(LoadedProgram.Motions.ToList(), preset, XMinMm, XLimitMm, YMinMm, YLimitMm);
        PlacementOffsetX = placement.OffsetX;
        PlacementOffsetY = placement.OffsetY;
        ApplyPlacement();
    }

    private void SyncSessionWithExecutionState()
    {
        switch (_executionQueueService.ExecutionState)
        {
            case CncExecutionState.Paused:
                if (JobSessionState != CncJobLifecycleState.Paused)
                    _jobSessionService.PauseSession("Operator paused the CNC job session.");
                break;
            case CncExecutionState.Completed:
                if (JobSessionState != CncJobLifecycleState.Completed)
                    _jobSessionService.CompleteSession(CompletedMotionCount, TotalMotionCount, MachineX, MachineY, MachineZ, "CNC job session completed successfully.");
                break;
            case CncExecutionState.Stopped:
                if (JobSessionState != CncJobLifecycleState.Stopped)
                    _jobSessionService.StopSession(CompletedMotionCount, TotalMotionCount, MachineX, MachineY, MachineZ, _executionQueueService.LastInterruptionReason ?? "CNC job session stopped by operator.");
                break;
            case CncExecutionState.Error:
                if (JobSessionState is not CncJobLifecycleState.Failed and not CncJobLifecycleState.Interrupted)
                {
                    var message = _executionQueueService.LastInterruptionReason ?? _cncControllerService.LastFaultReason ?? "CNC job session failed.";
                    _jobSessionService.FailSession(CompletedMotionCount, TotalMotionCount, MachineX, MachineY, MachineZ, message);
                }
                break;
        }
    }

    private string BuildSessionSummary()
    {
        var session = CurrentJobSession ?? LastJobSessionSummary;
        if (session == null)
            return "No operator session summary available.";

        var endedAt = session.EndedAt ?? session.InterruptedAt;
        var endedText = endedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "In progress";
        return $"{FormatJobSessionState(session.SessionState)} | Started {session.StartedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Not started"} | Ended {endedText} | Duration {FormatDuration(session.Duration)} | {session.CompletedLines}/{session.TotalLines} lines | Last position X {session.LastKnownX:0.###} Y {session.LastKnownY:0.###} Z {session.LastKnownZ:0.###}";
    }

    private string BuildEffectiveCapabilitiesSummary()
    {
        var caps = _activeMachineContextService.Current.EffectiveCapabilities;
        var enabled = new List<string>();

        if (caps.Motion.Homing) enabled.Add("Homing");
        if (caps.Motion.CenterMove) enabled.Add("Center Move");
        if (caps.Motion.WorkOffset) enabled.Add("Work Zero");
        if (caps.Execution.ToolpathPreview) enabled.Add("Preview");
        if (caps.Execution.Frame) enabled.Add("Frame");
        if (caps.Execution.FileRun) enabled.Add("File Run");
        if (caps.Execution.Simulation) enabled.Add("Simulation");
        if (caps.Protocol.Acknowledgements) enabled.Add("Acknowledgements");

        return enabled.Count == 0
            ? "No effective runtime capabilities resolved yet."
            : string.Join("  |  ", enabled);
    }

    private static MachineDefinition BuildDefinitionFromCncProfile(CncMachineProfile profile)
    {
        var driverType = profile.DriverType == CncDriverType.Simulated ? DriverType.Simulated : DriverType.ArduinoSerial;
        return new MachineDefinition
        {
            Id = profile.MachineDefinitionId ?? Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Code = "legacy-cnc-profile",
            Version = profile.MachineDefinitionVersion ?? "legacy-local",
            DisplayNameEn = profile.ProfileName,
            DisplayNameAr = profile.ProfileName,
            DescriptionEn = profile.Description,
            Manufacturer = "MABA",
            IsActive = true,
            IsPublic = false,
            RuntimeBinding = new RuntimeBindingSection
            {
                DefaultDriverType = driverType,
                SupportedDriverTypes = new List<DriverType> { driverType },
                FirmwareProtocol = driverType == DriverType.Simulated ? FirmwareProtocol.Custom : FirmwareProtocol.MabaProtocol,
                SupportedSetupModes = driverType == DriverType.Simulated ? new List<SetupMode> { SetupMode.SimulationOnly } : new List<SetupMode> { SetupMode.RealOnly },
                VisualizationType = VisualizationType.CncTopDown2D,
                KinematicsType = KinematicsType.MovingGantryXY,
                RuntimeUiVariant = "cnc-standard-v1"
            },
            AxisConfig = new AxisConfigSection
            {
                AxisCount = new[] { profile.SupportsXAxis, profile.SupportsYAxis, profile.SupportsZAxis }.Count(v => v),
                SupportedAxes = new[] { (profile.SupportsXAxis, AxisId.X), (profile.SupportsYAxis, AxisId.Y), (profile.SupportsZAxis, AxisId.Z) }
                    .Where(x => x.Item1)
                    .Select(x => x.Item2)
                    .ToList(),
                HomingSupport = new Dictionary<string, bool> { ["X"] = profile.HomeXEnabled, ["Y"] = profile.HomeYEnabled, ["Z"] = profile.HomeZEnabled },
                HomeOriginConvention = MachineHomeOriginConvention.FrontLeft,
                WorkCoordinateSupport = true,
                MachineCoordinateSupport = true,
                RelativeMoveSupport = true,
                AbsoluteMoveSupport = true
            },
            Workspace = new WorkspaceSection
            {
                MinTravelMm = new Dictionary<string, double> { ["X"] = (double)profile.XMinMm, ["Y"] = (double)profile.YMinMm, ["Z"] = (double)profile.ZMinMm },
                MaxTravelMm = new Dictionary<string, double> { ["X"] = (double)profile.XLimitMm, ["Y"] = (double)profile.YLimitMm, ["Z"] = (double)profile.ZLimitMm },
                WorkAreaMm = new WorkAreaDimensions { Width = (double)(profile.XLimitMm - profile.XMinMm), Depth = (double)(profile.YLimitMm - profile.YMinMm), Height = (double)(profile.ZLimitMm - profile.ZMinMm) }
            },
            MotionDefaults = new MotionDefaultsSection
            {
                StepsPerMm = new Dictionary<string, double> { ["X"] = (double)profile.XStepsPerMm, ["Y"] = (double)profile.YStepsPerMm, ["Z"] = (double)profile.ZStepsPerMm },
                JogPresets = profile.JogPresets.Select(v => new JogPreset { Label = $"{v:0.###} mm", DistanceMm = (double)v, FeedMmMin = 600 }).ToList()
            },
            ConnectionDefaults = new ConnectionDefaultsSection
            {
                DefaultBaudRate = profile.BaudRate,
                SupportedBaudRates = new List<int> { profile.BaudRate },
                SupportedConnectionTypes = driverType == DriverType.Simulated ? new List<ConnectionType> { ConnectionType.Simulated } : new List<ConnectionType> { ConnectionType.Serial },
                RequiresHandshake = driverType != DriverType.Simulated
            },
            Capabilities = new CapabilitiesSection
            {
                Motion = new MotionCapabilities { Homing = profile.HomeXEnabled || profile.HomeYEnabled, ZHoming = profile.HomeZEnabled, CombinedXYHoming = profile.HomeXEnabled && profile.HomeYEnabled, RelativeMoves = true, AbsoluteMoves = true, Pause = true, Resume = true, Stop = true, CenterMove = profile.SupportsXAxis && profile.SupportsYAxis, WorkOffset = true, JogStep = true },
                Execution = new ExecutionCapabilities { RealExecution = driverType != DriverType.Simulated, Simulation = driverType == DriverType.Simulated, PreviewPlayback = true, FileRun = true, Frame = true, BoundingBoxPreview = true, EstimatedPositionOnly = true, ToolpathPreview = true, ProgressTracking = true },
                Protocol = new ProtocolCapabilities { Handshake = true, Acknowledgements = true, AlarmReporting = true, AlarmReset = true, StatusQuery = true, MotorEnable = true, MotorDisable = true, SoftReset = true },
                Visualization = new VisualizationCapabilities { MachineVisualization = true, TopView2D = true, Perspective3D = true, KinematicsAnimation = true, RealTimePositionDisplay = true },
                FileHandling = new FileHandlingCapabilities { LocalFileRun = true, StreamingExecution = true, GcodeValidation = true, MultipleFileFormats = true }
            },
            ProfileRules = new ProfileRulesSection
            {
                AllowedOverrides = new List<OverrideField> { OverrideField.DriverType, OverrideField.BaudRate, OverrideField.StepsPerMm, OverrideField.JogPresets, OverrideField.Notes }
            }
        };
    }

    private void PlayPreview()
    {
        try
        {
            _previewPlaybackService.Play();
            LastFeedback = "Preview simulation started.";
            RefreshPreviewState();
        }
        catch (Exception ex)
        {
            HandleUiError(ex.Message, "Start Preview", logAsWarning: true);
        }
    }

    private void PausePreview()
    {
        _previewPlaybackService.Pause();
        LastFeedback = "Preview simulation paused.";
        RefreshPreviewState();
    }

    private void StopPreview()
    {
        _previewPlaybackService.Stop();
        LastFeedback = "Preview simulation reset.";
        RefreshPreviewState();
    }

    private void StartFramePreview()
    {
        try
        {
            if (!_frameBounds.IsValid)
                throw new InvalidOperationException("Load a valid G-code job before running frame preview.");

            var frameMotions = _framePathService.BuildFramePath(_frameBounds);
            _previewPlaybackService.PlayFrame(frameMotions);
            LastFeedback = "Frame preview simulation started.";
            RefreshPreviewState();
        }
        catch (Exception ex)
        {
            HandleUiError(ex.Message, "Start Frame Preview", logAsWarning: true);
        }
    }

    private void RefreshPreviewState()
    {
        OnPropertyChanged(nameof(PreviewMotions));
        OnPropertyChanged(nameof(PreviewSimulationState));
        OnPropertyChanged(nameof(PreviewSimulationStateDisplay));
        OnPropertyChanged(nameof(PreviewCurrentSegmentIndex));
        OnPropertyChanged(nameof(PreviewSegmentProgress));
        OnPropertyChanged(nameof(PreviewToolX));
        OnPropertyChanged(nameof(PreviewToolY));
        OnPropertyChanged(nameof(PreviewCurrentLineText));
        OnPropertyChanged(nameof(PreviewTotalDistanceMm));
        OnPropertyChanged(nameof(PreviewRapidDistanceMm));
        OnPropertyChanged(nameof(PreviewCutDistanceMm));
        OnPropertyChanged(nameof(PreviewEstimatedTime));
        OnPropertyChanged(nameof(PreviewPlaybackSpeed));
        OnPropertyChanged(nameof(PreviewPlaybackSpeedDisplay));
        OnPropertyChanged(nameof(IsFramePreviewActive));
        OnPropertyChanged(nameof(UsePreviewPlaybackInViewport));
        OnPropertyChanged(nameof(HasFrameBounds));
        OnPropertyChanged(nameof(FrameMinX));
        OnPropertyChanged(nameof(FrameMaxX));
        OnPropertyChanged(nameof(FrameMinY));
        OnPropertyChanged(nameof(FrameMaxY));
        OnPropertyChanged(nameof(CanFramePreview));
        OnPropertyChanged(nameof(CanPlayPreview));
        OnPropertyChanged(nameof(CanPausePreview));
        OnPropertyChanged(nameof(CanStopPreview));
        CommandManager.InvalidateRequerySuggested();
    }

    private void RefreshWorkflowState()
    {
        OnPropertyChanged(nameof(OperatorEvents));
        OnPropertyChanged(nameof(LoadedJobInfo));
        OnPropertyChanged(nameof(HasLoadedJobInfo));
        OnPropertyChanged(nameof(LoadedJobTitle));
        OnPropertyChanged(nameof(LoadedJobSourceReference));
        OnPropertyChanged(nameof(LoadedJobPath));
        OnPropertyChanged(nameof(LoadedJobMotionCount));
        OnPropertyChanged(nameof(LoadedJobProfileName));
        OnPropertyChanged(nameof(LoadedJobDriverMode));
        OnPropertyChanged(nameof(JobSessionState));
        OnPropertyChanged(nameof(JobSessionStateDisplay));
        OnPropertyChanged(nameof(JobReadinessSummary));
        OnPropertyChanged(nameof(JobBlockingReason));
        OnPropertyChanged(nameof(HasBlockingReason));
        OnPropertyChanged(nameof(IsReadyToRun));
        OnPropertyChanged(nameof(HasCurrentSession));
        OnPropertyChanged(nameof(CurrentJobSession));
        OnPropertyChanged(nameof(SessionStartedAtDisplay));
        OnPropertyChanged(nameof(SessionEndedAtDisplay));
        OnPropertyChanged(nameof(SessionLastAction));
        OnPropertyChanged(nameof(SessionResultText));
        OnPropertyChanged(nameof(SessionDurationDisplay));
        OnPropertyChanged(nameof(SessionSummaryText));
        OnPropertyChanged(nameof(HasLastJobSessionSummary));
        OnPropertyChanged(nameof(LastJobSessionSummary));
    }

    private void RefreshState()
    {
        SyncWorkflowContext();
        UpdateReadinessState();
        OnPropertyChanged(nameof(IsConnected));
        OnPropertyChanged(nameof(MotorsEnabled));
        OnPropertyChanged(nameof(IsHomed));
        OnPropertyChanged(nameof(HasValidMachineReference));
        OnPropertyChanged(nameof(HasAlarm));
        OnPropertyChanged(nameof(HasWarning));
        OnPropertyChanged(nameof(ConnectionStatusText));
        OnPropertyChanged(nameof(MachineStateDisplay));
        OnPropertyChanged(nameof(CurrentAlarmText));
        OnPropertyChanged(nameof(CurrentWarningText));
        OnPropertyChanged(nameof(PlacementStatus));
        OnPropertyChanged(nameof(PlacementOffsetX));
        OnPropertyChanged(nameof(PlacementOffsetY));
        OnPropertyChanged(nameof(AppliedPlacementOffsetX));
        OnPropertyChanged(nameof(AppliedPlacementOffsetY));
        OnPropertyChanged(nameof(DeviceReadyDisplay));
        OnPropertyChanged(nameof(DeviceStateDisplay));
        OnPropertyChanged(nameof(DriverCapabilitiesSummary));
        OnPropertyChanged(nameof(ActiveProfileName));
        OnPropertyChanged(nameof(ActiveProfileDriver));
        OnPropertyChanged(nameof(ActiveProfileBaudRateDisplay));
        OnPropertyChanged(nameof(IsSimulationMode));
        OnPropertyChanged(nameof(IsHardwarePortRequired));
        OnPropertyChanged(nameof(RuntimeModeText));
        OnPropertyChanged(nameof(LoadedJobInfo));
        OnPropertyChanged(nameof(HasLoadedJobInfo));
        OnPropertyChanged(nameof(LoadedJobTitle));
        OnPropertyChanged(nameof(LoadedJobSourceReference));
        OnPropertyChanged(nameof(LoadedJobPath));
        OnPropertyChanged(nameof(LoadedJobMotionCount));
        OnPropertyChanged(nameof(LoadedJobProfileName));
        OnPropertyChanged(nameof(LoadedJobDriverMode));
        OnPropertyChanged(nameof(OperatorEvents));
        OnPropertyChanged(nameof(JobSessionState));
        OnPropertyChanged(nameof(JobSessionStateDisplay));
        OnPropertyChanged(nameof(JobReadinessSummary));
        OnPropertyChanged(nameof(JobBlockingReason));
        OnPropertyChanged(nameof(HasBlockingReason));
        OnPropertyChanged(nameof(IsReadyToRun));
        OnPropertyChanged(nameof(HasCurrentSession));
        OnPropertyChanged(nameof(CurrentJobSession));
        OnPropertyChanged(nameof(SessionStartedAtDisplay));
        OnPropertyChanged(nameof(SessionEndedAtDisplay));
        OnPropertyChanged(nameof(SessionLastAction));
        OnPropertyChanged(nameof(SessionResultText));
        OnPropertyChanged(nameof(SessionDurationDisplay));
        OnPropertyChanged(nameof(SessionSummaryText));
        OnPropertyChanged(nameof(HasLastJobSessionSummary));
        OnPropertyChanged(nameof(LastJobSessionSummary));
        OnPropertyChanged(nameof(PositionTrackingDisplay));
        OnPropertyChanged(nameof(LastAcknowledgementText));
        OnPropertyChanged(nameof(ProtocolStatusText));
        OnPropertyChanged(nameof(ProtocolErrorText));
        OnPropertyChanged(nameof(DeviceReportedPositionText));
        OnPropertyChanged(nameof(CanJog));
        OnPropertyChanged(nameof(CanGoToCenter));
        OnPropertyChanged(nameof(MachineX));
        OnPropertyChanged(nameof(MachineY));
        OnPropertyChanged(nameof(MachineZ));
        OnPropertyChanged(nameof(WorkX));
        OnPropertyChanged(nameof(WorkY));
        OnPropertyChanged(nameof(WorkZ));
        OnPropertyChanged(nameof(WorkOffsetX));
        OnPropertyChanged(nameof(WorkOffsetY));
        OnPropertyChanged(nameof(WorkOffsetZ));
        OnPropertyChanged(nameof(MachineStateValue));
        OnPropertyChanged(nameof(CurrentMotionIndex));
        OnPropertyChanged(nameof(CanStartProgram));
        OnPropertyChanged(nameof(CanPauseProgram));
        OnPropertyChanged(nameof(CanResumeProgram));
        OnPropertyChanged(nameof(CanStopExecution));
        OnPropertyChanged(nameof(CanResetState));
        OnPropertyChanged(nameof(CanApplySelectedProfile));
        RefreshProfilePermissionState();
        OnPropertyChanged(nameof(CanApplyPlacement));
        OnPropertyChanged(nameof(CanFramePreview));
        OnPropertyChanged(nameof(CanPlayPreview));
        OnPropertyChanged(nameof(CanPausePreview));
        OnPropertyChanged(nameof(CanStopPreview));
        CommandManager.InvalidateRequerySuggested();
    }

    private void RefreshExecutionState()
    {
        OnPropertyChanged(nameof(ExecutionStateDisplay));
        OnPropertyChanged(nameof(ExecutionStateValue));
        OnPropertyChanged(nameof(CurrentLineNumber));
        OnPropertyChanged(nameof(CurrentRawLine));
        OnPropertyChanged(nameof(CompletedMotionCount));
        OnPropertyChanged(nameof(RemainingMotionCount));
        OnPropertyChanged(nameof(ProgressSummary));
        OnPropertyChanged(nameof(CurrentMotionIndex));
        RefreshState();
    }

    private void RefreshActiveJob()
    {
        SyncWorkflowContext();
        OnPropertyChanged(nameof(ActiveJob));
        OnPropertyChanged(nameof(HasActiveJob));
        OnPropertyChanged(nameof(ActiveJobTitle));
        OnPropertyChanged(nameof(ActiveJobReference));
    }

    private void HandleUiError(string message, string title, bool logAsWarning)
    {
        LastFeedback = message;
        AddDiagnostic(logAsWarning ? "Warning" : "Error", message);
        if (!logAsWarning)
        {
            MessageBox.Show(Application.Current.MainWindow, message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private static string FormatState(CncMachineState state)
    {
        return state switch
        {
            CncMachineState.Disconnected => "Disconnected",
            CncMachineState.Idle => "Idle",
            CncMachineState.Homing => "Homing",
            CncMachineState.Running => "Running",
            CncMachineState.Paused => "Paused",
            CncMachineState.Stopped => "Stopped",
            CncMachineState.Completed => "Completed",
            CncMachineState.Warning => "Warning",
            CncMachineState.Alarm => "Alarm",
            CncMachineState.Error => "Error",
            _ => state.ToString()
        };
    }

    private static string FormatExecutionState(CncExecutionState state)
    {
        return state switch
        {
            CncExecutionState.Idle => "Idle",
            CncExecutionState.Running => "Running",
            CncExecutionState.Paused => "Paused",
            CncExecutionState.Stopped => "Stopped",
            CncExecutionState.Error => "Error",
            CncExecutionState.Completed => "Completed",
            _ => state.ToString()
        };
    }

    private static string FormatJobSessionState(CncJobLifecycleState state)
    {
        return state switch
        {
            CncJobLifecycleState.NoJob => "No Job",
            CncJobLifecycleState.Loaded => "Loaded",
            CncJobLifecycleState.Ready => "Ready",
            CncJobLifecycleState.Running => "Running",
            CncJobLifecycleState.Paused => "Paused",
            CncJobLifecycleState.Stopped => "Stopped",
            CncJobLifecycleState.Completed => "Completed",
            CncJobLifecycleState.Failed => "Failed",
            CncJobLifecycleState.Interrupted => "Interrupted",
            _ => state.ToString()
        };
    }

    private static string FormatPreviewSimulationState(CncPreviewSimulationState state)
    {
        return state switch
        {
            CncPreviewSimulationState.Idle => "Idle",
            CncPreviewSimulationState.Ready => "Ready",
            CncPreviewSimulationState.Playing => "Playing",
            CncPreviewSimulationState.Paused => "Paused",
            CncPreviewSimulationState.Completed => "Completed",
            _ => state.ToString()
        };
    }

    private static string FormatDuration(TimeSpan? duration)
    {
        return duration.HasValue ? duration.Value.ToString(@"hh\:mm\:ss") : "N/A";
    }

    private static string FormatDeviceState(CncDeviceState state)
    {
        return state switch
        {
            CncDeviceState.Unknown => "Unknown",
            CncDeviceState.Ready => "Ready",
            CncDeviceState.Idle => "Idle",
            CncDeviceState.Running => "Running",
            CncDeviceState.Homing => "Homing",
            CncDeviceState.Paused => "Paused",
            CncDeviceState.Stopped => "Stopped",
            CncDeviceState.Alarm => "Alarm",
            CncDeviceState.Error => "Error",
            CncDeviceState.LimitHit => "Limit Hit",
            CncDeviceState.Disconnected => "Disconnected",
            _ => state.ToString()
        };
    }
}

public enum MachineWizardStep
{
    Catalog = 1,
    Family = 2,
    Machine = 3,
    Mode = 4,
    Confirm = 5
}

public sealed class MachineWizardStepItem : ViewModelBase
{
    private bool _isCurrent;
    private bool _isCompleted;

    public MachineWizardStepItem(MachineWizardStep step, string title)
    {
        Step = step;
        Title = title;
    }

    public MachineWizardStep Step { get; }
    public string Title { get; }

    public bool IsCurrent
    {
        get => _isCurrent;
        set { if (_isCurrent == value) return; _isCurrent = value; OnPropertyChanged(); }
    }

    public bool IsCompleted
    {
        get => _isCompleted;
        set { if (_isCompleted == value) return; _isCompleted = value; OnPropertyChanged(); }
    }
}

public sealed class MachineWizardCard : ViewModelBase
{
    private bool _isSelected;

    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string VersionText { get; set; } = string.Empty;
    public string WorkspaceText { get; set; } = string.Empty;
    public string AxesText { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public bool HasImage => IsDirectImageUrl(ImageUrl);
    public string ImagePlaceholderText => string.IsNullOrWhiteSpace(ImageUrl) ? "No image" : "Image URL is not a direct image";
    public string IconText { get; set; } = "▣";
    public ObservableCollection<string> Tags { get; set; } = new();
    public MachineFamily? Family { get; set; }
    public MachineDefinitionSummary? MachineSummary { get; set; }
    public MachineDefinition? MachineDefinition { get; set; }
    public SetupMode? SetupMode { get; set; }

    public bool IsSelected
    {
        get => _isSelected;
        set { if (_isSelected == value) return; _isSelected = value; OnPropertyChanged(); }
    }

    private static bool IsDirectImageUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var trimmed = value.Trim();
        if (trimmed.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
            return true;

        var path = trimmed;
        if (Uri.TryCreate(trimmed, UriKind.Absolute, out var uri))
            path = uri.LocalPath;

        var extension = Path.GetExtension(path);
        return extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".png", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".gif", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".bmp", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".webp", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".svg", StringComparison.OrdinalIgnoreCase);
    }
}
