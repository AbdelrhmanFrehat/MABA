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
    private readonly ICncRuntimeCoordinator _runtimeCoordinator;
    private readonly ICncCoordinateTransformService _coordinateTransformService;
    private readonly ICncManagerService _cncManagerService;
    private string? _lastRecoverySignature;
    private bool _hadRecoveryPlan;

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
    private readonly List<GcodeInterpretedCommand> _placedInterpretedCommands = new();
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
    private bool _isMachineSwitchOpen;
    private bool _isProfileManagerOpen;
    private bool _isUploadingFirmware;
    private MachineWizardStep _wizardStep = MachineWizardStep.Catalog;
    private MachineWizardCard? _wizardSelectedFamilyCard;
    private MachineWizardCard? _wizardSelectedMachineCard;
    private MachineWizardCard? _wizardSelectedModeCard;
    private SetupMode? _wizardSelectedSetupMode;
    private ExistingMachineCard? _selectedExistingMachineCard;

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
        IActiveMachineContextService activeMachineContextService,
        ICncRuntimeCoordinator runtimeCoordinator,
        ICncCoordinateTransformService coordinateTransformService,
        ICncManagerService cncManagerService)
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
        _runtimeCoordinator = runtimeCoordinator;
        _coordinateTransformService = coordinateTransformService;
        _cncManagerService = cncManagerService;

        AvailablePorts = new ObservableCollection<string>();
        JogStepPresets = new ObservableCollection<decimal>();
        DiagnosticsMessages = new ObservableCollection<string>();
        WizardSteps = new ObservableCollection<MachineWizardStepItem>();
        WizardFamilyCards = new ObservableCollection<MachineWizardCard>();
        WizardMachineCards = new ObservableCollection<MachineWizardCard>();
        WizardModeCards = new ObservableCollection<MachineWizardCard>();
        ExistingMachineCards = new ObservableCollection<ExistingMachineCard>();
        DriverTypes = Enum.GetValues(typeof(CncDriverType)).Cast<CncDriverType>().ToList();
        HomeOriginOptions = Enum.GetValues(typeof(CncHomeOriginConvention)).Cast<CncHomeOriginConvention>().ToList();

        RefreshPortsCommand = new RelayCommand(_ => RefreshPorts());
        ConnectCommand = new RelayCommand(_ => ConnectMachine(), _ => CanConnectMachine);
        DisconnectCommand = new RelayCommand(_ => DisconnectMachine(), _ => CanDisconnectMachine);
        EnableMotorsCommand = new RelayCommand(_ => UnlockMachine(), _ => CanEnableOrUnlock);
        DisableMotorsCommand = new RelayCommand(_ => DisableMotors(), _ => CanDisableMotors);
        HomeCommand = new RelayCommand(_ => HomeMachine(), _ => CanHome);
        GoToCenterCommand = new RelayCommand(_ => MoveToCenter(), _ => CanGoToCenter);
        SetZeroCommand = new RelayCommand(_ => SetWorkZero(), _ => CanSetZero);
        SetZeroXCommand = new RelayCommand(_ => SetWorkZeroX(), _ => CanSetZero);
        SetZeroYCommand = new RelayCommand(_ => SetWorkZeroY(), _ => CanSetZero);
        SetZeroXyCommand = new RelayCommand(_ => SetWorkZeroXY(), _ => CanSetZero);
        ClearWorkZeroCommand = new RelayCommand(_ => ClearWorkZero(), _ => CanClearWorkZero);
        ResetStateCommand = new RelayCommand(_ => ResetControllerState(), _ => CanResetState);
        ClearWarningCommand = new RelayCommand(_ => RunAction(_cncControllerService.ClearWarning), _ => HasWarning);
        StopCommand = new RelayCommand(async _ => await StopExecutionAsync(), _ => CanStopExecution);
        RefreshStatusCommand = new RelayCommand(_ => RefreshControllerStatus(), _ => CanRefreshStatus);
        ReconnectRecoveryCommand = new RelayCommand(_ => ReconnectForRecovery(), _ => CanReconnectForRecovery);
        RestartJobCommand = new RelayCommand(async _ => await RestartJobAsync(), _ => CanRestartJob);
        AbortJobCommand = new RelayCommand(async _ => await AbortJobAsync(), _ => CanAbortJob);
        CopyDiagnosticsCommand = new RelayCommand(_ => CopyFirmwareDiagnostics(), _ => CanCopyDiagnostics);
        SaveConfigCommand = new RelayCommand(_ => SaveConfig(), _ => CanSaveSelectedProfile);
        ApplyProfileCommand = new RelayCommand(_ => ApplySelectedProfile(), _ => CanApplySelectedProfile);
        DuplicateProfileCommand = new RelayCommand(_ => DuplicateSelectedProfile(), _ => SelectedProfile != null);
        DeleteProfileCommand = new RelayCommand(_ => DeleteSelectedProfile(), _ => CanDeleteSelectedProfile);
        RestoreDefaultProfilesCommand = new RelayCommand(_ => RestoreDefaultProfiles());
        SyncMachineCatalogCommand = new RelayCommand(async _ => await SyncMachineCatalogAsync());
        CreateRuntimeProfileCommand = new RelayCommand(async _ => await CreateRuntimeProfileFromSelectionAsync(), _ => SelectedMachineDefinitionSummary != null);
        OpenMachineWizardCommand = new RelayCommand(_ => OpenMachineWizard());
        OpenMachineSwitchCommand = new RelayCommand(_ => OpenMachineSwitch());
        CloseMachineWizardCommand = new RelayCommand(_ => CloseMachineWizard());
        CloseMachineSwitchCommand = new RelayCommand(_ => CloseMachineSwitch());
        WizardBackCommand = new RelayCommand(_ => MoveWizardBack(), _ => CanMoveWizardBack);
        WizardNextCommand = new RelayCommand(async _ => await MoveWizardNextAsync(), _ => CanMoveWizardNext);
        SelectWizardFamilyCommand = new RelayCommand(SelectWizardFamily);
        SelectWizardMachineCommand = new RelayCommand(SelectWizardMachine);
        SelectWizardModeCommand = new RelayCommand(SelectWizardMode);
        ConfirmMachineWizardCommand = new RelayCommand(async _ => await ConfirmMachineWizardAsync(), _ => WizardSelectedMachineDefinition != null);
        SelectExistingMachineCommand = new RelayCommand(SelectExistingMachine);
        ConfirmExistingMachineSwitchCommand = new RelayCommand(_ => ConfirmExistingMachineSwitch(), _ => SelectedExistingMachineCard?.RuntimeProfile != null);
        ManageProfilesCommand = new RelayCommand(_ => IsProfileManagerOpen = !IsProfileManagerOpen);
        UseSelectedMachineCommand = new RelayCommand(_ => ConfirmAndActivateSelectedMachine(), _ => SelectedMachineDefinitionSummary != null && SelectedRuntimeProfile != null);
        ActivateRuntimeProfileCommand = new RelayCommand(_ => ActivateSelectedRuntimeProfile(), _ => SelectedRuntimeProfile != null);
        DuplicateRuntimeProfileCommand = new RelayCommand(_ => DuplicateSelectedRuntimeProfile(), _ => SelectedRuntimeProfile != null);
        DeleteRuntimeProfileCommand = new RelayCommand(_ => DeleteSelectedRuntimeProfile(), _ => CanDeleteSelectedRuntimeProfile);
        UploadArduinoFirmwareCommand = new RelayCommand(async _ => await UploadArduinoFirmwareAsync(), _ => CanUploadArduinoFirmware);
        BackToJobsCommand = new RelayCommand(_ => _navigationService.NavigateTo("Jobs"), _ => ActiveJob != null);
        JogXPositiveCommand = new RelayCommand(_ => Jog("X", SelectedJogStep), _ => CanJog);
        JogXNegativeCommand = new RelayCommand(_ => Jog("X", -SelectedJogStep), _ => CanJog);
        JogYPositiveCommand = new RelayCommand(_ => Jog("Y", SelectedJogStep), _ => CanJog);
        JogYNegativeCommand = new RelayCommand(_ => Jog("Y", -SelectedJogStep), _ => CanJog);
        JogZPositiveCommand = new RelayCommand(_ => Jog("Z", SelectedJogStep), _ => CanJog);
        JogZNegativeCommand = new RelayCommand(_ => Jog("Z", -SelectedJogStep), _ => CanJog);
        LoadGcodeCommand = new RelayCommand(_ => LoadGcodeFile(), _ => CanLoadGcode);
        ClearLoadedProgramCommand = new RelayCommand(_ => ClearLoadedProgram(), _ => CanClearLoadedProgram);
        ApplyPlacementCommand = new RelayCommand(_ => ApplyPlacement(), _ => CanApplyPlacement);
        ResetPlacementCommand = new RelayCommand(_ => ResetPlacement(), _ => CanApplyPlacement);
        PlaceTopLeftCommand = new RelayCommand(_ => ApplyPlacementPreset(CncPlacementPreset.TopLeft), _ => CanApplyPlacement);
        PlaceTopRightCommand = new RelayCommand(_ => ApplyPlacementPreset(CncPlacementPreset.TopRight), _ => CanApplyPlacement);
        PlaceBottomLeftCommand = new RelayCommand(_ => ApplyPlacementPreset(CncPlacementPreset.BottomLeft), _ => CanApplyPlacement);
        PlaceBottomRightCommand = new RelayCommand(_ => ApplyPlacementPreset(CncPlacementPreset.BottomRight), _ => CanApplyPlacement);
        PlaceCenterCommand = new RelayCommand(_ => ApplyPlacementPreset(CncPlacementPreset.Center), _ => CanApplyPlacement);
        FramePreviewCommand = new RelayCommand(async _ => await StartFramePreviewAsync(), _ => CanFramePreview);
        PlayPreviewCommand = new RelayCommand(_ => PlayPreview(), _ => CanPlayPreview);
        PausePreviewCommand = new RelayCommand(_ => PausePreview(), _ => CanPausePreview);
        StopPreviewCommand = new RelayCommand(_ => StopPreview(), _ => CanStopPreview);
        StartProgramCommand = new RelayCommand(async _ => await StartProgramAsync(), _ => CanStartProgram);
        PauseProgramCommand = new RelayCommand(_ => PauseProgram(), _ => CanPauseProgram);
        ResumeProgramCommand = new RelayCommand(async _ => await ResumeProgramAsync(), _ => CanResumeProgram);

        _cncControllerService.StateChanged += (_, _) =>
        {
            RefreshActiveMachineContext();
            RefreshState();
        };
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
        _runtimeCoordinator.StatusChanged += (_, _) => RefreshRuntimeStatus();
        _jobSessionService.SessionChanged += (_, _) => RefreshWorkflowState();
        _previewPlaybackService.PlaybackChanged += (_, _) => RefreshPreviewState();
        _executionQueueService.ExecutionStateChanged += (_, _) =>
        {
            if (!string.IsNullOrWhiteSpace(_executionQueueService.LastInterruptionReason))
            {
                var severity = _executionQueueService.ExecutionState is CncExecutionState.Error or CncExecutionState.Failed or CncExecutionState.Alarmed ? "Error" : "Info";
                AddDiagnostic(severity, _executionQueueService.LastInterruptionReason);
            }

            SyncSessionWithExecutionState();
            RefreshExecutionState();
        };

        _runtimeProfileService.EnsureSystemProfilesForDefinitions(_machineCatalogService.CachedDefinitions.ToList());
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
    public ObservableCollection<ExistingMachineCard> ExistingMachineCards { get; }
    public IReadOnlyList<CncDriverType> DriverTypes { get; }
    public IReadOnlyList<CncHomeOriginConvention> HomeOriginOptions { get; }
    public string MachineCatalogSyncStatus => _machineCatalogService.LastSyncStatus;
    public string MachinePlatformStatus
    {
        get => _machinePlatformStatus;
        private set { if (_machinePlatformStatus == value) return; _machinePlatformStatus = value; OnPropertyChanged(); }
    }
    private CapabilitiesSection EffectiveCapabilities => _activeMachineContextService.Current.EffectiveCapabilities;
    private CncFirmwareCapabilities? ActiveFirmwareCapabilities => RuntimeStatus.FirmwareIdentity?.Capabilities;
    public CncRuntimeStatus RuntimeStatus => _runtimeCoordinator.Current;
    public string ActiveMachineContextText => _activeMachineContextService.Current.StatusText;
    public string EffectiveCapabilitiesSummary => BuildEffectiveCapabilitiesSummary();
    public bool HasSelectedRuntimeProfile => SelectedRuntimeProfile != null;
    public bool HasSelectedMachineDefinition => SelectedMachineDefinitionSummary != null;
    public bool IsMachineWizardOpen
    {
        get => _isMachineWizardOpen;
        set { if (_isMachineWizardOpen == value) return; _isMachineWizardOpen = value; OnPropertyChanged(); }
    }
    public bool IsMachineSwitchOpen
    {
        get => _isMachineSwitchOpen;
        set { if (_isMachineSwitchOpen == value) return; _isMachineSwitchOpen = value; OnPropertyChanged(); }
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
    public ExistingMachineCard? SelectedExistingMachineCard
    {
        get => _selectedExistingMachineCard;
        set
        {
            if (ReferenceEquals(_selectedExistingMachineCard, value))
                return;

            _selectedExistingMachineCard = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasExistingMachines));
            CommandManager.InvalidateRequerySuggested();
        }
    }
    public bool HasExistingMachines => ExistingMachineCards.Count > 0;
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
    public ICommand SetZeroXCommand { get; }
    public ICommand SetZeroYCommand { get; }
    public ICommand SetZeroXyCommand { get; }
    public ICommand ClearWorkZeroCommand { get; }
    public ICommand ResetStateCommand { get; }
    public ICommand ClearWarningCommand { get; }
    public ICommand StopCommand { get; }
    public ICommand RefreshStatusCommand { get; }
    public ICommand ReconnectRecoveryCommand { get; }
    public ICommand RestartJobCommand { get; }
    public ICommand AbortJobCommand { get; }
    public ICommand CopyDiagnosticsCommand { get; }
    public ICommand SaveConfigCommand { get; }
    public ICommand ApplyProfileCommand { get; }
    public ICommand DuplicateProfileCommand { get; }
    public ICommand DeleteProfileCommand { get; }
    public ICommand RestoreDefaultProfilesCommand { get; }
    public ICommand SyncMachineCatalogCommand { get; }
    public ICommand CreateRuntimeProfileCommand { get; }
    public ICommand OpenMachineWizardCommand { get; }
    public ICommand OpenMachineSwitchCommand { get; }
    public ICommand CloseMachineWizardCommand { get; }
    public ICommand CloseMachineSwitchCommand { get; }
    public ICommand WizardBackCommand { get; }
    public ICommand WizardNextCommand { get; }
    public ICommand SelectWizardFamilyCommand { get; }
    public ICommand SelectWizardMachineCommand { get; }
    public ICommand SelectWizardModeCommand { get; }
    public ICommand ConfirmMachineWizardCommand { get; }
    public ICommand SelectExistingMachineCommand { get; }
    public ICommand ConfirmExistingMachineSwitchCommand { get; }
    public ICommand ManageProfilesCommand { get; }
    public ICommand UseSelectedMachineCommand { get; }
    public ICommand ActivateRuntimeProfileCommand { get; }
    public ICommand DuplicateRuntimeProfileCommand { get; }
    public ICommand DeleteRuntimeProfileCommand { get; }
    public ICommand UploadArduinoFirmwareCommand { get; }
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
    public string RuntimeStateDisplay => RuntimeStatus.RuntimeStateDisplay;
    public string RuntimeModeDisplay => RuntimeStatus.ControllerModeDisplay;
    public bool HasRuntimeAlarmBanner => RuntimeStatus.IsAlarmed;
    public string RuntimeAlarmBannerText => RuntimeStatus.LastAlarmMessage;
    public bool HasRuntimeLockBanner => RuntimeStatus.IsLocked && !RuntimeStatus.IsAlarmed;
    public string RuntimeLockBannerText => RuntimeStatus.BlockingReason ?? "Machine locked — unlock or home required.";
    public CncRecoveryPlan RecoveryPlan => RuntimeStatus.RecoveryPlan;
    public bool HasRecoveryPlan => RecoveryPlan.HasRecovery;
    public string RecoveryStateText => RecoveryPlan.StateDisplay;
    public string RecoverySeverityText => RecoveryPlan.SeverityDisplay;
    public string RecoverySummaryText => RecoveryPlan.Summary;
    public string RecoveryRequiredActionText => RecoveryPlan.RequiredNextAction;
    public int RecoveryFailedLineNumber => RecoveryPlan.FailedSourceLine ?? 0;
    public string RecoveryFailedCommandText => string.IsNullOrWhiteSpace(RecoveryPlan.FailedCommandText) ? "No failed command captured." : RecoveryPlan.FailedCommandText!;
    public bool HasRecoveryFailedLine => RecoveryPlan.FailedSourceLine.HasValue;
    public bool HasRecoveryControllerMessage => !string.IsNullOrWhiteSpace(RecoveryPlan.ControllerMessage);
    public string RecoveryControllerMessageText => RecoveryPlan.ControllerMessage ?? "No controller recovery message.";
    public bool CanRecoveryRefreshStatus => RecoveryPlan.Allows(CncRecoveryAction.RefreshStatus) && CanRefreshStatus;
    public bool CanRecoveryUnlock => RecoveryPlan.Allows(CncRecoveryAction.UnlockController) && CanEnableOrUnlock;
    public bool CanRecoveryReconnect => RecoveryPlan.Allows(CncRecoveryAction.Reconnect) && CanConnectMachine;
    public bool CanRecoveryClearAlarm => RecoveryPlan.Allows(CncRecoveryAction.ClearAlarm) && CanResetState;
    public bool CanRecoveryHome => RecoveryPlan.Allows(CncRecoveryAction.RehomeMachine) && CanHome;
    public bool CanRecoveryClearJob => RecoveryPlan.Allows(CncRecoveryAction.ClearJob) && CanClearLoadedProgram;
    public bool CanRecoveryResume => RecoveryPlan.Allows(CncRecoveryAction.ResumeJob) && CanResumeProgram;
    public bool CanRecoveryRestart => RecoveryPlan.Allows(CncRecoveryAction.RestartJob) && CanRestartJob;
    public bool CanRecoveryAbort => RecoveryPlan.Allows(CncRecoveryAction.AbortJob) && CanAbortJob;
    public bool CanRecoveryResetWorkOffset => RecoveryPlan.Allows(CncRecoveryAction.ResetWorkOffset) && CanClearWorkZero;
    public bool HasRuntimeBlockingReason => !string.IsNullOrWhiteSpace(RuntimeStatus.BlockingReason);
    public string RuntimeBlockingReasonText => RuntimeStatus.BlockingReason ?? "No blocking issues.";
    public string RuntimeFirmwareVersionText => RuntimeStatus.FirmwareVersion ?? "Unknown";
    public string RuntimeProtocolVersionText => RuntimeStatus.ProtocolVersion ?? "Unknown";
    public string RuntimeFirmwareNameText => RuntimeStatus.FirmwareIdentity.FirmwareName;
    public string RuntimeFirmwareConfidenceText => RuntimeStatus.FirmwareIdentity.Confidence.ToString();
    public string RuntimeCompatibilityStatusText => RuntimeStatus.FirmwareCompatibility.Status.ToString();
    public string RuntimeCompatibilitySummaryText
    {
        get
        {
            if (RuntimeStatus.FirmwareCompatibility.Errors.Count > 0)
                return RuntimeStatus.FirmwareCompatibility.Errors[0];
            if (RuntimeStatus.FirmwareCompatibility.Warnings.Count > 0)
                return RuntimeStatus.FirmwareCompatibility.Warnings[0];
            if (RuntimeStatus.FirmwareIdentity.FirmwareWarnings.Count > 0)
                return RuntimeStatus.FirmwareIdentity.FirmwareWarnings[0];
            return "No firmware compatibility warnings.";
        }
    }
    public bool HasRuntimeCompatibilityNotes =>
        RuntimeStatus.FirmwareCompatibility.Errors.Count > 0
        || RuntimeStatus.FirmwareCompatibility.Warnings.Count > 0
        || RuntimeStatus.FirmwareIdentity.FirmwareWarnings.Count > 0;
    public bool CanCopyDiagnostics => HasDiagnostics || HasRuntimeCompatibilityNotes;
    public string RuntimeProgressPercentText => $"{RuntimeStatus.ProgressPercent:0.#}%";
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
    public string ActiveGcodeUnits => LoadedProgram?.FinalModalState.Units == GcodeUnitMode.Inches ? "Inch (G20)" : "Millimeter (G21)";
    public string ActiveGcodeDistanceMode => LoadedProgram?.FinalModalState.DistanceMode == GcodeDistanceMode.Incremental ? "Incremental (G91)" : "Absolute (G90)";
    public string ActiveGcodePlane => LoadedProgram?.FinalModalState.Plane switch
    {
        GcodePlane.XZ => "XZ (G18)",
        GcodePlane.YZ => "YZ (G19)",
        _ => "XY (G17)"
    };
    public int UnsupportedCommandCount => LoadedProgram?.UnsupportedCommandCount ?? 0;
    public string InterpretationSummary => LoadedProgram == null
        ? "No interpreted G-code loaded."
        : $"{LoadedProgram.InterpretedCommands.Count} interpreted line(s) | {UnsupportedCommandCount} unsupported command(s)";
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
            OnPropertyChanged(nameof(CanUploadArduinoFirmware));
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
    public bool CanConnectMachine => !IsConnected
                                     && (IsSimulationMode || !string.IsNullOrWhiteSpace(SelectedPort))
                                     && GetActionDescriptor(CncRuntimeAction.Connect).IsAllowed;
    public bool CanDisconnectMachine => IsConnected && GetActionDescriptor(CncRuntimeAction.Disconnect).IsAllowed;
    public string ConnectionStatusText => IsConnected
        ? $"Connected on {_cncControllerService.ConnectedPort}"
        : "Disconnected";
    public string MachineStateDisplay => FormatState(_cncControllerService.MachineState);

    public bool CanJog => GetActionDescriptor(CncRuntimeAction.Jog).IsAllowed && EffectiveCapabilities.Motion.JogStep;
    public bool HasUnlockCapability => ActiveFirmwareCapabilities?.SupportsUnlock == true
                                       || RecoveryPlan.Allows(CncRecoveryAction.UnlockController);
    public bool HasMotorEnableCapability => EffectiveCapabilities.Protocol.MotorEnable;
    public bool HasMotorDisableCapability => EffectiveCapabilities.Protocol.MotorDisable;
    public bool ShowEnableOrUnlockControl => HasMotorEnableCapability || HasUnlockCapability;
    public bool ShowDisableMotorsControl => HasMotorDisableCapability;
    public bool ShowStatusControl => RuntimeStatus.IsConnected || EffectiveCapabilities.Protocol.StatusQuery;
    public string EnableOrUnlockButtonText => HasMotorEnableCapability ? "Enable Motors" : "Unlock Controller";
    public bool CanEnableOrUnlock => (HasMotorEnableCapability || HasUnlockCapability)
                                     && GetActionDescriptor(CncRuntimeAction.Unlock).IsAllowed;
    public bool CanEnableMotors => EffectiveCapabilities.Protocol.MotorEnable && GetActionDescriptor(CncRuntimeAction.Unlock).IsAllowed;
    public bool CanDisableMotors => EffectiveCapabilities.Protocol.MotorDisable && GetActionDescriptor(CncRuntimeAction.DisableMotors).IsAllowed;
    public bool CanHome => EffectiveCapabilities.Motion.Homing && GetActionDescriptor(CncRuntimeAction.Home).IsAllowed;
    public bool CanGoToCenter => EffectiveCapabilities.Motion.CenterMove
                                 && SupportsXAxis
                                 && SupportsYAxis
                                 && GetActionDescriptor(CncRuntimeAction.GoToCenter).IsAllowed;
    public bool CanSetZero => EffectiveCapabilities.Motion.WorkOffset && GetActionDescriptor(CncRuntimeAction.SetWorkZero).IsAllowed;
    public bool CanClearWorkZero => EffectiveCapabilities.Motion.WorkOffset && GetActionDescriptor(CncRuntimeAction.ClearWorkZero).IsAllowed;
    public bool CanRefreshStatus => RuntimeStatus.IsConnected
                                    && GetActionDescriptor(CncRuntimeAction.RefreshStatus).IsAllowed;

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
    public string DiagnosticsSummary => $"{TotalMotionCount} interpreted moves | {UnsupportedCommandCount} unsupported | {WarningCount} warnings | {ErrorCount} errors";
    public int WarningCount => DiagnosticsMessages.Count(m => m.StartsWith("[Warning]"));
    public int ErrorCount => DiagnosticsMessages.Count(m => m.StartsWith("[Error]"));
    public string DeviceReadyDisplay => _cncControllerService.DeviceStatus.IsReady ? "Ready" : "Waiting / Not Ready";
    public string DeviceStateDisplay => FormatDeviceState(_cncControllerService.DeviceStatus.DeviceState);
    public string PositionTrackingDisplay => _cncControllerService.DeviceStatus.PositionTrackingMode;
    public string LastAcknowledgementText => _cncControllerService.DeviceStatus.LastAcknowledgement ?? "No acknowledgement received yet.";
    public string ProtocolStatusText => _cncControllerService.DeviceStatus.LastStatusText ?? "No protocol status received yet.";
    public string ProtocolErrorText => _cncControllerService.DeviceStatus.LastProtocolError ?? "No active protocol error.";
    public string ControllerStatusConfidenceText => RuntimeStatus.ControllerStatusConfidence.ToString();
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
    public decimal PlacementOffsetZ => 0m;
    public decimal AppliedPlacementOffsetZ => 0m;
    public string ReferenceStatusText => RuntimeStatus.ReferenceStatusDisplay;
    public string ReferenceWarningText => RuntimeStatus.ReferenceWarningText ?? "Machine reference is valid.";
    public bool HasReferenceWarning => !string.IsNullOrWhiteSpace(RuntimeStatus.ReferenceWarningText);
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
            OnPropertyChanged(nameof(CanClearLoadedProgram));
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
    public bool CanClearLoadedProgram => HasLoadedProgram && _runtimeCoordinator.CanExecute(CncRuntimeAction.ClearLoadedJob, out _);
    public string LoadedFileName => LoadedProgram?.FileName ?? "No G-code file loaded";
    public string LoadedFileSummary => LoadedProgram == null
        ? "Load a .gcode, .nc, or .txt file to build the execution queue."
        : $"{LoadedProgram.Motions.Count} motion line(s) from {LoadedProgram.TotalLines} file line(s)";
    public IEnumerable<GcodeMotionCommand> MotionCommands => _placedMotions;
    public int TotalMotionCount => MotionCommands.Count(m => m.IsExecutable);
    public int CurrentLineNumber => _executionQueueService.CurrentPlannedCommand?.SourceLineNumber ?? _executionQueueService.CurrentMotion?.LineNumber ?? 0;
    public string CurrentRawLine => _executionQueueService.CurrentMotion?.RawText ?? "No motion line executing.";
    public string CurrentStreamedCommand => _executionQueueService.CurrentPlannedCommand?.CommandText ?? "No controller command streaming.";
    public string StreamingStateDisplay => FormatStreamingState(_executionQueueService.StreamingState);
    public int FailedLineNumber => _executionQueueService.FailedPlannedCommand?.SourceLineNumber
                                   ?? _executionQueueService.Diagnostics.FailedSourceLine
                                   ?? 0;
    public string FailedStreamedCommand => _executionQueueService.FailedPlannedCommand?.CommandText
                                           ?? _executionQueueService.Diagnostics.FailedCommandText
                                           ?? "No failed command.";
    public int CompletedMotionCount => _executionQueueService.CompletedCount;
    public int RemainingMotionCount => Math.Max(0, TotalMotionCount - CompletedMotionCount);
    public string ProgressSummary => _executionQueueService.CurrentStreamingSession == null
        ? $"{CompletedMotionCount} completed / {TotalMotionCount} total / {RemainingMotionCount} remaining"
        : $"{_executionQueueService.CurrentStreamingSession.CompletedCommands} completed / {_executionQueueService.CurrentStreamingSession.TotalCommands} streamed / {Math.Max(0, _executionQueueService.CurrentStreamingSession.TotalCommands - _executionQueueService.CurrentStreamingSession.CompletedCommands)} remaining";
    public string StreamingProgressSummary => _executionQueueService.CurrentStreamingSession == null
        ? "No active streaming session."
        : $"{_executionQueueService.CurrentStreamingSession.ProgressPercent:0.#}% | {_executionQueueService.CurrentStreamingSession.StreamedDistanceMm:0.###} mm streamed | Ack avg {_executionQueueService.Diagnostics.AverageAckMilliseconds:0.#} ms";
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
    public bool HasToolpathPreviewCapability => EffectiveCapabilities.Execution.ToolpathPreview;
    public bool HasMachineVisualizationCapability => EffectiveCapabilities.Visualization.MachineVisualization;
    public bool HasProgressTrackingCapability => EffectiveCapabilities.Execution.ProgressTracking;
    public bool HasLiveReportedPositionCapability => EffectiveCapabilities.Protocol.PositionQuery || EffectiveCapabilities.Execution.LiveReportedPosition;
    public bool CanFramePreview => HasLoadedProgram && HasFrameBounds && EffectiveCapabilities.Execution.Frame && GetActionDescriptor(CncRuntimeAction.Frame).IsAllowed;
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
    public bool CanApplyPlacement => HasLoadedProgram && _executionQueueService.ExecutionState is not (CncExecutionState.PreflightChecking or CncExecutionState.ReadyToRun or CncExecutionState.Running or CncExecutionState.Paused or CncExecutionState.Stopping);
    public bool CanLoadGcode => EffectiveCapabilities.FileHandling.LocalFileRun && GetActionDescriptor(CncRuntimeAction.LoadJob).IsAllowed;
    public bool CanPlayPreview => HasLoadedProgram && MotionCommands.Any() && EffectiveCapabilities.Execution.ToolpathPreview && EffectiveCapabilities.Execution.PreviewPlayback && PreviewSimulationState is CncPreviewSimulationState.Ready or CncPreviewSimulationState.Paused or CncPreviewSimulationState.Completed;
    public bool CanPausePreview => PreviewSimulationState == CncPreviewSimulationState.Playing;
    public bool CanStopPreview => PreviewSimulationState is CncPreviewSimulationState.Playing or CncPreviewSimulationState.Paused or CncPreviewSimulationState.Completed;
    public bool CanStartProgram => EffectiveCapabilities.Execution.FileRun
                                   && HasLoadedProgram
                                   && TotalMotionCount > 0
                                   && ErrorCount == 0
                                   && GetActionDescriptor(CncRuntimeAction.Run).IsAllowed;
    public bool CanPauseProgram => GetActionDescriptor(CncRuntimeAction.Pause).IsAllowed;
    public bool CanResumeProgram => GetActionDescriptor(CncRuntimeAction.Resume).IsAllowed;
    public bool CanStopExecution => EffectiveCapabilities.Motion.Stop && GetActionDescriptor(CncRuntimeAction.Stop).IsAllowed;
    public bool CanReconnectForRecovery => CanConnectMachine;
    public bool CanRestartJob => HasLoadedProgram && GetActionDescriptor(CncRuntimeAction.Run).IsAllowed;
    public bool CanAbortJob => HasLoadedProgram && !IsUploadingFirmware && (RuntimeStatus.IsBusy || RecoveryPlan.HasRecovery);
    public bool CanResetState => (EffectiveCapabilities.Protocol.AlarmReset || EffectiveCapabilities.Protocol.SoftReset)
                                 && GetActionDescriptor(CncRuntimeAction.ResetAlarm).IsAllowed;
    public bool CanApplySelectedProfile => SelectedProfile != null && SelectedProfile.ProfileId != _cncProfileService.ActiveProfile.ProfileId;
    public bool IsSelectedProfileBuiltIn => SelectedProfile?.IsBuiltIn == true;
    public bool CanEditSelectedProfile => SelectedProfile?.IsEditable == true;
    public bool CanSaveSelectedProfile => CanEditSelectedProfile;
    public bool CanDeleteSelectedProfile => SelectedProfile != null && SelectedProfile.IsEditable && !SelectedProfile.IsDefault && _cncProfileService.Profiles.Count > 1;
    public bool CanDeleteSelectedRuntimeProfile => SelectedRuntimeProfile?.ProfileType == RuntimeProfileType.User;
    public bool HasBundledArduinoFirmware => ArduinoFirmwarePackage.Exists();
    public bool IsUploadingFirmware => _isUploadingFirmware;
    public string BundledFirmwareVersion => ArduinoFirmwarePackage.FirmwareVersion;
    public string BundledFirmwareTargetBoard => ArduinoFirmwarePackage.TargetBoardDisplay;
    public string ArduinoFirmwareToolingStatus => ArduinoFirmwareUploader.ToolingStatus;
    public bool CanUploadArduinoFirmware => HasBundledArduinoFirmware
                                            && ArduinoFirmwareUploader.CanUpload
                                            && !IsSimulationMode
                                            && !string.IsNullOrWhiteSpace(SelectedPort)
                                            && !_isUploadingFirmware
                                            && _runtimeCoordinator.CanExecute(CncRuntimeAction.UploadFirmware, out _);
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

    private void RunRuntimeAction(CncRuntimeAction runtimeAction, Action action, string title, bool logAsWarning = false, bool recovering = false)
    {
        try
        {
            EnsureRuntimeActionAllowed(runtimeAction);
            if (recovering)
                _runtimeCoordinator.SetRecovering(true);

            action();
            LastFeedback = "Command completed.";
            RefreshState();
        }
        catch (Exception ex)
        {
            HandleUiError(ex.Message, title, logAsWarning);
            RefreshState();
        }
        finally
        {
            if (recovering)
                _runtimeCoordinator.SetRecovering(false);
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

    private void RunRuntimeResponseAction(CncRuntimeAction runtimeAction, Func<string> action, string title, bool logAsWarning = false, bool recovering = false)
    {
        try
        {
            EnsureRuntimeActionAllowed(runtimeAction);
            if (recovering)
                _runtimeCoordinator.SetRecovering(true);

            LastFeedback = action();
            RefreshState();
        }
        catch (Exception ex)
        {
            HandleUiError(ex.Message, title, logAsWarning);
            RefreshState();
        }
        finally
        {
            if (recovering)
                _runtimeCoordinator.SetRecovering(false);
        }
    }

    private CncRuntimeActionDescriptor GetActionDescriptor(CncRuntimeAction action)
    {
        return RuntimeStatus.GetActionDescriptor(action);
    }

    private void EnsureRuntimeActionAllowed(CncRuntimeAction action)
    {
        var descriptor = GetActionDescriptor(action);
        if (descriptor.IsAllowed)
            return;

        throw new InvalidOperationException(descriptor.Reason ?? $"'{action}' is not allowed right now.");
    }

    private void ConnectMachine()
    {
        try
        {
            LastFeedback = _cncManagerService.Connect(SelectedPort);
        }
        catch (Exception ex)
        {
            HandleUiError(ex.Message, "Connect CNC Machine", logAsWarning: false);
        }
        finally
        {
            RefreshState();
        }
    }

    private void DisconnectMachine()
    {
        RunRuntimeAction(CncRuntimeAction.Disconnect, () => _cncManagerService.Disconnect(), "Disconnect CNC Machine");
    }

    private void UnlockMachine()
    {
        RunRuntimeResponseAction(CncRuntimeAction.Unlock, () => _cncManagerService.Unlock(), "Unlock CNC Machine", recovering: HasAlarm || RuntimeStatus.IsLocked);
    }

    private void DisableMotors()
    {
        RunRuntimeResponseAction(CncRuntimeAction.DisableMotors, () => _cncControllerService.DisableMotors(), "Disable Motors");
    }

    private void HomeMachine()
    {
        RunRuntimeResponseAction(CncRuntimeAction.Home, () => _cncManagerService.Home(), "Home CNC Machine", recovering: RuntimeStatus.IsLocked || RuntimeStatus.IsAlarmed);
    }

    private void MoveToCenter()
    {
        RunRuntimeResponseAction(CncRuntimeAction.GoToCenter, () => _cncControllerService.GoToCenter(), "Move To Center");
    }

    private void SetWorkZero()
    {
        SetWorkZeroXY();
    }

    private void SetWorkZeroX()
    {
        RunRuntimeResponseAction(CncRuntimeAction.SetWorkZero, () => _cncManagerService.SetWorkZeroX(), "Set Work Zero X");
    }

    private void SetWorkZeroY()
    {
        RunRuntimeResponseAction(CncRuntimeAction.SetWorkZero, () => _cncManagerService.SetWorkZeroY(), "Set Work Zero Y");
    }

    private void SetWorkZeroXY()
    {
        RunRuntimeResponseAction(CncRuntimeAction.SetWorkZero, () => _cncManagerService.SetWorkZeroXY(), "Set Work Zero XY");
    }

    private void ClearWorkZero()
    {
        RunRuntimeResponseAction(CncRuntimeAction.ClearWorkZero, () => _cncManagerService.ClearWorkZero(), "Clear Work Zero");
    }

    private void ResetControllerState()
    {
        RunRuntimeResponseAction(CncRuntimeAction.ResetAlarm, () => _cncControllerService.ResetState(), "Reset Controller State", recovering: true);
    }

    private void RefreshControllerStatus()
    {
        RunRuntimeResponseAction(CncRuntimeAction.RefreshStatus, () => _cncManagerService.RefreshStatus(), "Refresh Controller Status");
    }

    private void Jog(string axis, decimal deltaMm)
    {
        try
        {
            LastFeedback = _cncManagerService.Jog(axis, deltaMm);
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
        if (!_runtimeCoordinator.CanExecute(CncRuntimeAction.LoadJob, out var blockingReason))
        {
            HandleUiError(blockingReason ?? "Loading a job is not allowed right now.", "Load G-code", logAsWarning: true);
            return;
        }

        var dialog = new OpenFileDialog
        {
            Filter = "G-code files (*.gcode;*.nc;*.txt)|*.gcode;*.nc;*.txt|All files (*.*)|*.*",
            Title = "Load G-code"
        };

        if (dialog.ShowDialog(Application.Current.MainWindow) != true)
            return;

        try
        {
            _runtimeCoordinator.SetLoadingJob(true);
            ClearDiagnostics();
            LastFeedback = "Parsing G-code file...";
            AddDiagnostic("Info", $"Loading file: {dialog.FileName}");
            var parsed = _gcodeParserService.ParseFile(dialog.FileName);
            LoadedProgram = parsed;
            _jobPlacement = new CncJobPlacement();
            PlacementOffsetX = 0m;
            PlacementOffsetY = 0m;
            _runtimeCoordinator.SetJobPlacementOffset(new CncJobPlacementOffset());
            _jobSessionService.LoadJob(parsed, ActiveProfileName, RuntimeModeText, ActiveJob?.Title, ActiveJob?.JobReference);
            _runtimeCoordinator.SetJobLoaded(parsed.FileName, true);
            RevalidateLoadedProgram();
            LastFeedback = $"Loaded {LoadedProgram.FileName}.";
            AddDiagnostic("Info", $"Parse completed with {LoadedProgram.Motions.Count} motion line(s) and {LoadedProgram.InterpretedCommands.Count} interpreted line(s).");
            AddDiagnostics(LoadedProgram.Messages, defaultSeverity: "Warning");
        }
        catch (Exception ex)
        {
            HandleUiError(ex.Message, "Load G-code", logAsWarning: false);
        }
        finally
        {
            _runtimeCoordinator.SetLoadingJob(false);
            RefreshState();
        }
    }

    private void ClearLoadedProgram()
    {
        if (!_runtimeCoordinator.CanExecute(CncRuntimeAction.ClearLoadedJob, out var blockingReason))
        {
            HandleUiError(blockingReason ?? "Clearing the loaded job is not allowed right now.", "Clear G-code", logAsWarning: true);
            return;
        }

        LoadedProgram = null;
        _jobSessionService.ClearLoadedJob();
        _runtimeCoordinator.SetJobLoaded(null, false);
        _previewPlaybackService.Stop();
        _previewPlaybackService.Load(Array.Empty<GcodeMotionCommand>());
        _placedMotions.Clear();
        _frameBounds = new CncFrameBounds();
        _jobPlacement = new CncJobPlacement();
        PlacementOffsetX = 0m;
        PlacementOffsetY = 0m;
        _runtimeCoordinator.SetJobPlacementOffset(new CncJobPlacementOffset());
        PlacementStatus = "Placement offsets are zeroed.";
        ClearDiagnostics();
        _executionQueueService.Load(Array.Empty<GcodeMotionCommand>(), LoadedProgram?.FileName, Array.Empty<GcodeInterpretedCommand>());
        LastFeedback = "Loaded G-code cleared.";
        AddDiagnostic("Info", "Loaded G-code cleared.");
        RefreshWorkflowState();
    }

    private async Task StartProgramAsync()
    {
        try
        {
            var preflight = GetExecutionPreflight(CncExecutionIntent.Run);
            if (!preflight.IsAllowed)
                throw new InvalidOperationException(preflight.Summary ?? "Execution preflight failed.");

            RevalidateLoadedProgram();
            if (ErrorCount > 0 || _placedMotions.Any(m => !m.IsValid))
                throw new InvalidOperationException("Critical validation issues are blocking execution.");

            var result = await _cncManagerService.RunAsync(request: new CncExecutionPreflightRequest
            {
                Intent = CncExecutionIntent.Run,
                RuntimeStatus = RuntimeStatus,
                MachineConfig = _cncControllerService.Config,
                MachineBounds = _cncControllerService.Bounds,
                LoadedProgram = LoadedProgram,
                MotionCommands = _placedMotions.ToList(),
                InterpretedCommands = _placedInterpretedCommands.ToList(),
                ParserErrorCount = ErrorCount
            }, motions: _placedMotions.ToList(), commands: _placedInterpretedCommands.ToList(), activeJobName: LoadedProgram?.FileName, totalMotionCount: TotalMotionCount);
            if (!result.Success && !string.IsNullOrWhiteSpace(result.Error))
                throw new InvalidOperationException(result.Error);

            LastFeedback = result.Message;
            AddDiagnostic("Info", result.Message);
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
        try
        {
            _cncManagerService.Pause();
        }
        catch (Exception ex)
        {
            HandleUiError(ex.Message, "Pause G-code Execution", logAsWarning: true);
            return;
        }

        LastFeedback = "Execution paused.";
        AddDiagnostic("Info", "Execution paused.");
        RefreshState();
    }

    private async Task ResumeProgramAsync()
    {
        try
        {
            var result = await _cncManagerService.ResumeAsync();
            if (!result.Success && !string.IsNullOrWhiteSpace(result.Error))
                throw new InvalidOperationException(result.Error);
            LastFeedback = result.Message;
            AddDiagnostic("Info", result.Message);
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
            var result = await _cncManagerService.StopAsync();
            LastFeedback = result.Message;
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

    private void ReconnectForRecovery()
    {
        AddDiagnostic("Warning", "Recovery action selected: reconnect controller.");
        ConnectMachine();
    }

    private async Task RestartJobAsync()
    {
        try
        {
            if (!CanRecoveryRestart)
                throw new InvalidOperationException(RecoveryPlan.RequiredNextAction);

            AddDiagnostic("Warning", "Recovery action selected: restart job from beginning.");
            var request = new CncExecutionPreflightRequest
            {
                Intent = CncExecutionIntent.Run,
                RuntimeStatus = RuntimeStatus,
                MachineConfig = _cncControllerService.Config,
                MachineBounds = _cncControllerService.Bounds,
                LoadedProgram = LoadedProgram,
                MotionCommands = _placedMotions.ToList(),
                InterpretedCommands = _placedInterpretedCommands.ToList(),
                ParserErrorCount = ErrorCount
            };
            var result = await _cncManagerService.RestartAsync(request, _placedMotions.ToList(), _placedInterpretedCommands.ToList(), LoadedProgram?.FileName, TotalMotionCount);
            if (!result.Success && !string.IsNullOrWhiteSpace(result.Error))
                throw new InvalidOperationException(result.Error);
            LastFeedback = result.Message;
        }
        catch (Exception ex)
        {
            HandleUiError(ex.Message, "Restart CNC Job", logAsWarning: false);
        }
        finally
        {
            RefreshState();
        }
    }

    private async Task AbortJobAsync()
    {
        try
        {
            AddDiagnostic("Warning", "Recovery action selected: abort job.");
            var result = await _cncManagerService.AbortAsync();
            LastFeedback = result.Message;
        }
        catch (Exception ex)
        {
            HandleUiError(ex.Message, "Abort CNC Job", logAsWarning: false);
        }
        finally
        {
            RefreshState();
        }
    }

    private void CopyFirmwareDiagnostics()
    {
        var lines = new List<string>
        {
            $"Firmware: {RuntimeFirmwareNameText}",
            $"Version: {RuntimeFirmwareVersionText}",
            $"Protocol: {RuntimeProtocolVersionText}",
            $"Confidence: {RuntimeFirmwareConfidenceText}",
            $"Compatibility: {RuntimeCompatibilityStatusText}",
            $"Summary: {RuntimeCompatibilitySummaryText}"
        };

        if (RuntimeStatus.FirmwareCompatibility.Errors.Count > 0)
            lines.AddRange(RuntimeStatus.FirmwareCompatibility.Errors.Select(error => $"Error: {error}"));
        if (RuntimeStatus.FirmwareCompatibility.Warnings.Count > 0)
            lines.AddRange(RuntimeStatus.FirmwareCompatibility.Warnings.Select(warning => $"Warning: {warning}"));
        if (RuntimeStatus.FirmwareIdentity.FirmwareWarnings.Count > 0)
            lines.AddRange(RuntimeStatus.FirmwareIdentity.FirmwareWarnings.Select(warning => $"Firmware warning: {warning}"));
        if (DiagnosticsMessages.Count > 0)
            lines.AddRange(DiagnosticsMessages);

        Clipboard.SetText(string.Join(Environment.NewLine, lines.Distinct()));
        LastFeedback = "Firmware and diagnostics copied to clipboard.";
    }

    private void ApplySelectedProfile()
    {
        if (SelectedProfile == null)
            return;

        if (_executionQueueService.ExecutionState is CncExecutionState.PreflightChecking or CncExecutionState.ReadyToRun or CncExecutionState.Running or CncExecutionState.Paused or CncExecutionState.Stopping)
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
        if (_executionQueueService.ExecutionState is CncExecutionState.PreflightChecking or CncExecutionState.ReadyToRun or CncExecutionState.Running or CncExecutionState.Paused or CncExecutionState.Stopping)
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
        IsMachineSwitchOpen = false;
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

    private void OpenMachineSwitch()
    {
        BuildExistingMachineCards();
        if (ExistingMachineCards.Count == 0)
        {
            MachinePlatformStatus = "No previously added machines were found. Add a machine first.";
            OpenMachineWizard();
            return;
        }

        IsMachineWizardOpen = false;
        IsMachineSwitchOpen = true;
        SelectedExistingMachineCard = ExistingMachineCards
            .FirstOrDefault(card => card.RuntimeProfile?.IsActive == true)
            ?? ExistingMachineCards.FirstOrDefault();

        foreach (var card in ExistingMachineCards)
            card.IsSelected = ReferenceEquals(card, SelectedExistingMachineCard);

        MachinePlatformStatus = "Choose an existing machine to switch the CNC workspace.";
    }

    private void CloseMachineSwitch()
    {
        IsMachineSwitchOpen = false;
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

    private void SelectExistingMachine(object? parameter)
    {
        if (parameter is not ExistingMachineCard card || card.RuntimeProfile == null)
            return;

        foreach (var item in ExistingMachineCards)
            item.IsSelected = ReferenceEquals(item, card);

        SelectedExistingMachineCard = card;
    }

    private void ConfirmExistingMachineSwitch()
    {
        if (SelectedExistingMachineCard?.RuntimeProfile == null)
            return;

        var runtimeProfile = SelectedExistingMachineCard.RuntimeProfile;
        var result = MessageBox.Show(
            Application.Current.MainWindow,
            $"Switch to this machine?\n\nMachine: {SelectedExistingMachineCard.Title}\nProfile: {runtimeProfile.ProfileName}\nDriver: {SelectedExistingMachineCard.DriverText}\n\nThis will activate the existing runtime profile without repeating machine setup.",
            "Switch Machine",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes)
            return;

        ActivateRuntimeProfile(runtimeProfile);
        CloseMachineSwitch();
        MachinePlatformStatus = $"Switched to existing machine '{SelectedExistingMachineCard.Title}'.";
        AddDiagnostic("Info", MachinePlatformStatus);
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
        BuildExistingMachineCards();
        OnPropertyChanged(nameof(HasExistingMachines));
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

    private void BuildExistingMachineCards()
    {
        ExistingMachineCards.Clear();

        var cards = _runtimeProfileService.Profiles
            .Select(runtimeProfile =>
            {
                var definition = _machineCatalogService.GetCachedDefinition(runtimeProfile.MachineDefinitionId, runtimeProfile.MachineDefinitionVersion)
                                 ?? runtimeProfile.DefinitionSnapshot;

                var title = definition?.DisplayNameEn ?? runtimeProfile.ProfileName;
                var subtitle = FirstNonEmpty(
                    definition?.FamilyDisplayNameEn,
                    runtimeProfile.ProfileType == RuntimeProfileType.System ? "Default system machine" : "Custom machine profile",
                    "Existing machine");
                var driver = runtimeProfile.Overrides.DriverType
                             ?? definition?.RuntimeBinding.DefaultDriverType
                             ?? DriverType.Unknown;
                var modeText = driver == DriverType.Simulated ? "Simulation" : "Real Hardware";

                return new ExistingMachineCard
                {
                    Title = title,
                    Subtitle = subtitle,
                    ProfileText = runtimeProfile.ProfileName,
                    ModeText = modeText,
                    DriverText = driver.ToString(),
                    StatusText = runtimeProfile.IsActive ? "Currently active" : "Available to switch",
                    RuntimeProfile = runtimeProfile,
                    IsSelected = SelectedExistingMachineCard?.RuntimeProfile?.RuntimeProfileId == runtimeProfile.RuntimeProfileId
                };
            })
            .OrderByDescending(card => card.RuntimeProfile?.IsActive == true)
            .ThenBy(card => card.Title)
            .ThenBy(card => card.ModeText)
            .ToList();

        foreach (var card in cards)
            ExistingMachineCards.Add(card);

        SelectedExistingMachineCard = cards.FirstOrDefault(card => card.IsSelected)
                                     ?? cards.FirstOrDefault();
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
        var firmwareIdentity = _cncControllerService.DeviceStatus.FirmwareIdentity.IsKnown
            ? _cncControllerService.DeviceStatus.FirmwareIdentity
            : null;
        _activeMachineContextService.Resolve(runtimeProfile, liveDefinition, firmwareIdentity);
        _runtimeCoordinator.Refresh();
        OnPropertyChanged(nameof(ActiveMachineContextText));
        OnPropertyChanged(nameof(EffectiveCapabilitiesSummary));
        OnPropertyChanged(nameof(HasToolpathPreviewCapability));
        OnPropertyChanged(nameof(HasMachineVisualizationCapability));
        OnPropertyChanged(nameof(HasProgressTrackingCapability));
        OnPropertyChanged(nameof(HasLiveReportedPositionCapability));
        OnPropertyChanged(nameof(HasUnlockCapability));
        OnPropertyChanged(nameof(HasMotorEnableCapability));
        OnPropertyChanged(nameof(HasMotorDisableCapability));
        OnPropertyChanged(nameof(ShowEnableOrUnlockControl));
        OnPropertyChanged(nameof(ShowDisableMotorsControl));
        OnPropertyChanged(nameof(ShowStatusControl));
        OnPropertyChanged(nameof(EnableOrUnlockButtonText));
        OnPropertyChanged(nameof(CanEnableOrUnlock));
        OnPropertyChanged(nameof(CanEnableMotors));
        OnPropertyChanged(nameof(CanDisableMotors));
        OnPropertyChanged(nameof(CanHome));
        OnPropertyChanged(nameof(CanSetZero));
        OnPropertyChanged(nameof(CanRefreshStatus));
        OnPropertyChanged(nameof(CanLoadGcode));
        OnPropertyChanged(nameof(CanStopExecution));
        OnPropertyChanged(nameof(CanResetState));
        OnPropertyChanged(nameof(CanFramePreview));
        OnPropertyChanged(nameof(CanPlayPreview));
        OnPropertyChanged(nameof(CanPauseProgram));
        OnPropertyChanged(nameof(CanResumeProgram));
        OnPropertyChanged(nameof(CanStartProgram));
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
            _placedInterpretedCommands.Clear();
            _frameBounds = new CncFrameBounds();
            _jobSessionService.UpdateReadiness(false, "Load a G-code job to prepare a CNC session.", "No CNC job is currently loaded.");
            return;
        }

        ClearDiagnostics();

        _placedMotions.Clear();
        _placedInterpretedCommands.Clear();
        _placedMotions.AddRange(_jobPlacementService.ApplyPlacement(LoadedProgram.Motions.ToList(), _jobPlacement));
        _placedInterpretedCommands.AddRange(_jobPlacementService.ApplyPlacement(LoadedProgram.InterpretedCommands.ToList(), _jobPlacement));
        _frameBounds = _coordinateTransformService.ComputeFrameBounds(_placedMotions);

        foreach (var motion in _placedMotions)
        {
            motion.IsValid = true;
            motion.ValidationMessage = null;

            var transform = _coordinateTransformService.WorkToMachine(
                motion.EndX,
                motion.EndY,
                motion.EndZ,
                _cncControllerService.CoordinateState);
            var boundsMessage = _cncControllerService.ValidateMachinePosition(transform.FinalMachineX, transform.FinalMachineY, transform.FinalMachineZ);
            if (boundsMessage != null)
            {
                motion.IsValid = false;
                motion.ValidationMessage = $"Line {motion.LineNumber}: {boundsMessage} {transform.ExplainTransform()}";
            }

            if (!motion.IsValid && motion.ValidationMessage != null)
                AddDiagnostic("Error", motion.ValidationMessage);
        }

        AddDiagnostics(LoadedProgram.Messages, defaultSeverity: "Warning");
        _previewPlaybackService.Stop();
        _previewPlaybackService.Load(_placedMotions.ToList());
        _executionQueueService.Load(_placedMotions.ToList(), LoadedProgram?.FileName, _placedInterpretedCommands.ToList());
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
        OnPropertyChanged(nameof(CanCopyDiagnostics));
        LastError = "No current alarms.";
    }

    private async Task UploadArduinoFirmwareAsync()
    {
        try
        {
            EnsureRuntimeActionAllowed(CncRuntimeAction.UploadFirmware);
            if (!ArduinoFirmwarePackage.Exists())
                throw new InvalidOperationException("The bundled Arduino firmware package is missing from this app build.");
            if (IsSimulationMode)
                throw new InvalidOperationException("Switch to the real hardware machine profile before uploading firmware.");
            if (string.IsNullOrWhiteSpace(SelectedPort))
                throw new InvalidOperationException("Select the Arduino COM port before uploading firmware.");

            var port = SelectedPort!;
            var wasConnected = IsConnected;

            _isUploadingFirmware = true;
            _runtimeCoordinator.SetRecovering(true);
            OnPropertyChanged(nameof(IsUploadingFirmware));
            OnPropertyChanged(nameof(CanUploadArduinoFirmware));
            CommandManager.InvalidateRequerySuggested();

            if (wasConnected)
            {
                _cncControllerService.Disconnect();
                LastFeedback = "Machine disconnected temporarily for firmware upload.";
            }

            LastFeedback = $"Uploading Arduino firmware v{BundledFirmwareVersion} to {port}...";
            AddDiagnostic("Info", $"Firmware upload started on {port}.");
            var result = await ArduinoFirmwareUploader.UploadBundledFirmwareAsync(port);
            if (!result.Success)
                throw new InvalidOperationException(string.IsNullOrWhiteSpace(result.Output) ? result.Message : $"{result.Message}\n\n{result.Output}");

            LastFeedback = $"Firmware v{result.FirmwareVersion} uploaded successfully to {result.TargetBoard}.";
            AddDiagnostic("Info", $"Firmware upload completed on {port}.");

            if (wasConnected)
            {
                await Task.Delay(2200);
                _cncControllerService.Connect(port);
                LastFeedback = $"Firmware v{result.FirmwareVersion} uploaded and machine reconnected on {port}.";
            }
        }
        catch (Exception ex)
        {
            HandleUiError(ex.Message, "Upload Arduino Firmware", logAsWarning: false);
        }
        finally
        {
            _runtimeCoordinator.SetRecovering(false);
            _isUploadingFirmware = false;
            OnPropertyChanged(nameof(IsUploadingFirmware));
            OnPropertyChanged(nameof(CanUploadArduinoFirmware));
            OnPropertyChanged(nameof(ArduinoFirmwareToolingStatus));
            CommandManager.InvalidateRequerySuggested();
        }
    }

    private void AddDiagnostics(IEnumerable<string> messages, string defaultSeverity)
    {
        foreach (var message in messages)
        {
            if (string.IsNullOrWhiteSpace(message))
                continue;

            if (message.StartsWith("[") && message.Contains("]"))
            {
                var endBracket = message.IndexOf(']');
                if (endBracket > 1)
                {
                    var severity = message[1..endBracket];
                    var body = message[(endBracket + 1)..].Trim();
                    AddDiagnostic(severity, body);
                    continue;
                }
            }

            AddDiagnostic(defaultSeverity, message);
        }
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
        OnPropertyChanged(nameof(CanCopyDiagnostics));
    }

    private void SyncWorkflowContext()
    {
        _jobSessionService.UpdateRuntimeContext(ActiveProfileName, RuntimeModeText, ActiveJob?.Title, ActiveJob?.JobReference);
    }

    private void UpdateReadinessState()
    {
        var preflight = GetExecutionPreflight(CncExecutionIntent.Run);
        if (!preflight.IsAllowed)
        {
            var summary = RuntimeStatus.RuntimeState switch
            {
                CncRuntimeState.Locked => "Machine locked — unlock or home required.",
                CncRuntimeState.Alarm => "Machine alarm blocks execution.",
                CncRuntimeState.Disconnected => "Machine is not connected.",
                _ => preflight.Failures[0]
            };

            _jobSessionService.UpdateReadiness(false, summary, preflight.Failures[0]);
            return;
        }

        _jobSessionService.UpdateReadiness(true, "Ready to run. Machine, runtime state, and loaded job passed pre-run checks.", null);
    }

    private CncExecutionPreflightResult GetExecutionPreflight(CncExecutionIntent intent)
    {
        return _cncManagerService.EvaluatePreflight(new CncExecutionPreflightRequest
        {
            Intent = intent,
            RuntimeStatus = RuntimeStatus,
            MachineConfig = _cncControllerService.Config,
            MachineBounds = _cncControllerService.Bounds,
            LoadedProgram = LoadedProgram,
            MotionCommands = _placedMotions.ToList(),
            InterpretedCommands = _placedInterpretedCommands.ToList(),
            ParserErrorCount = ErrorCount
        });
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

        var validationMessage = _jobPlacementService.ValidatePlacement(
            LoadedProgram.Motions.Where(m => m.IsExecutable).ToList(),
            proposedPlacement,
            XMinMm,
            XLimitMm,
            YMinMm,
            YLimitMm);
        if (validationMessage != null)
        {
            HandleUiError(validationMessage, "Apply Job Placement", logAsWarning: true);
            return;
        }

        _jobPlacement = proposedPlacement;
        _runtimeCoordinator.SetJobPlacementOffset(new CncJobPlacementOffset
        {
            X = proposedPlacement.OffsetX,
            Y = proposedPlacement.OffsetY,
            Z = 0m
        });
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
            case CncExecutionState.Alarmed:
            case CncExecutionState.Error:
            case CncExecutionState.Failed:
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
                AxisDirections = new Dictionary<string, Direction> { ["X"] = Direction.Normal, ["Y"] = Direction.Normal, ["Z"] = Direction.Inverted },
                HomingSupport = new Dictionary<string, bool> { ["X"] = profile.HomeXEnabled, ["Y"] = profile.HomeYEnabled, ["Z"] = profile.HomeZEnabled },
                HomeOriginConvention = MachineHomeOriginConvention.FrontLeft,
                WorkCoordinateSupport = true,
                MachineCoordinateSupport = true,
                RelativeMoveSupport = true,
                AbsoluteMoveSupport = driverType == DriverType.Simulated
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
                RequiresHandshake = false,
                ResponseAckPattern = driverType == DriverType.Simulated ? "READY" : "HOME DONE",
                    ProtocolNotes = driverType == DriverType.Simulated
                        ? "Simulation profile."
                        : "MABA CNC motion firmware using ?, $H/$X, !, J jogs, G0/G1 linear moves, and G2/G3 arcs."
            },
            Capabilities = new CapabilitiesSection
            {
                Motion = new MotionCapabilities
                {
                    Homing = profile.HomeXEnabled || profile.HomeYEnabled,
                    ZHoming = profile.HomeZEnabled,
                    CombinedXYHoming = profile.HomeXEnabled && profile.HomeYEnabled,
                    RelativeMoves = true,
                    AbsoluteMoves = true,
                    Pause = driverType == DriverType.Simulated,
                    Resume = driverType == DriverType.Simulated,
                    Stop = true,
                    CenterMove = profile.SupportsXAxis && profile.SupportsYAxis,
                    WorkOffset = true,
                    JogStep = true
                },
                Execution = new ExecutionCapabilities
                {
                    RealExecution = driverType != DriverType.Simulated,
                    Simulation = driverType == DriverType.Simulated,
                    PreviewPlayback = true,
                    FileRun = true,
                    Frame = true,
                    BoundingBoxPreview = true,
                    EstimatedPositionOnly = driverType == DriverType.Simulated,
                    LiveReportedPosition = true,
                    ToolpathPreview = true,
                    ProgressTracking = true
                },
                Protocol = new ProtocolCapabilities
                {
                    Handshake = false,
                    Acknowledgements = true,
                    AlarmReporting = driverType != DriverType.Simulated,
                    AlarmReset = driverType != DriverType.Simulated,
                    StatusQuery = true,
                    PositionQuery = true,
                    MotorEnable = driverType != DriverType.Simulated,
                    MotorDisable = false,
                    SoftReset = false
                },
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

    private async Task StartFramePreviewAsync()
    {
        try
        {
            var preflight = GetExecutionPreflight(CncExecutionIntent.Frame);
            if (!preflight.IsAllowed)
                throw new InvalidOperationException(preflight.Summary ?? "Frame preflight failed.");
            if (!_frameBounds.IsValid)
                throw new InvalidOperationException("Load a valid G-code job before running frame preview.");

            var frameMotions = _framePathService.BuildFramePath(_frameBounds);
            _runtimeCoordinator.SetFraming(true);
            _previewPlaybackService.PlayFrame(frameMotions);
            LastFeedback = "Frame preview simulation started.";

            if (ShouldRunFrameOnMachine())
            {
                LastFeedback = "Frame preview started. Running physical frame on the machine...";
                AddDiagnostic("Info", "Physical frame started on the connected machine.");
                var result = await _cncManagerService.RunFrameAsync(
                    new CncExecutionPreflightRequest
                    {
                        Intent = CncExecutionIntent.Frame,
                        RuntimeStatus = RuntimeStatus,
                        MachineConfig = _cncControllerService.Config,
                        MachineBounds = _cncControllerService.Bounds,
                        LoadedProgram = LoadedProgram,
                        MotionCommands = _placedMotions.ToList(),
                        InterpretedCommands = _placedInterpretedCommands.ToList(),
                        ParserErrorCount = ErrorCount
                    },
                    frameMotions);
                if (!result.Success && !string.IsNullOrWhiteSpace(result.Error))
                    throw new InvalidOperationException(result.Error);
                LastFeedback = "Frame preview completed and the machine traced the loaded job bounds.";
                AddDiagnostic("Info", "Physical frame completed on the connected machine.");
            }

            RefreshPreviewState();
        }
        catch (Exception ex)
        {
            HandleUiError(ex.Message, "Start Frame Preview", logAsWarning: true);
        }
        finally
        {
            _runtimeCoordinator.SetFraming(false);
            RefreshState();
        }
    }

    private bool ShouldRunFrameOnMachine()
    {
        return IsConnected
               && !IsSimulationMode
               && MotorsEnabled
               && !HasAlarm
               && EffectiveCapabilities.Execution.RealExecution
               && EffectiveCapabilities.Execution.Frame;
    }

    private void RunFrameOnMachine(IReadOnlyList<GcodeMotionCommand> frameMotions)
    {
        if (frameMotions.Count == 0)
            return;

        foreach (var motion in frameMotions.Where(m => m.IsExecutable))
        {
            var targetX = motion.EndX;
            var targetY = motion.EndY;
            var targetZ = motion.EndZ;
            var boundsMessage = _cncControllerService.ValidateWorkPosition(targetX, targetY, targetZ);
            if (boundsMessage != null)
                throw new InvalidOperationException($"Frame blocked: {boundsMessage}");

            var deltaX = targetX - _cncControllerService.WorkX;
            var deltaY = targetY - _cncControllerService.WorkY;
            var deltaZ = targetZ - _cncControllerService.WorkZ;
            _cncControllerService.MoveLinear(deltaX, deltaY, deltaZ);
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
        OnPropertyChanged(nameof(ActiveGcodeUnits));
        OnPropertyChanged(nameof(ActiveGcodeDistanceMode));
        OnPropertyChanged(nameof(ActiveGcodePlane));
        OnPropertyChanged(nameof(UnsupportedCommandCount));
        OnPropertyChanged(nameof(InterpretationSummary));
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

    private void LogRecoveryPlanChanges()
    {
        var plan = RuntimeStatus.RecoveryPlan;
        var signature = $"{plan.State}|{plan.RequiredNextAction}|{plan.FailedSourceLine}|{plan.FailedCommandText}";
        if (signature == _lastRecoverySignature)
            return;

        if (_hadRecoveryPlan && !plan.HasRecovery)
            AddDiagnostic("Info", "Recovery complete. Machine state is back in a non-recovery state.");

        _lastRecoverySignature = signature;
        _hadRecoveryPlan = plan.HasRecovery;
        if (!plan.HasRecovery)
            return;

        var severity = plan.Severity switch
        {
            CncRecoverySeverity.Critical => "Error",
            CncRecoverySeverity.Warning => "Warning",
            _ => "Info"
        };

        AddDiagnostic(severity, $"Recovery: {plan.Summary}");
        if (!string.IsNullOrWhiteSpace(plan.RequiredNextAction))
            AddDiagnostic("Info", $"Next recovery step: {plan.RequiredNextAction}");
    }

    private void RefreshRuntimeStatus()
    {
        LogRecoveryPlanChanges();
        OnPropertyChanged(nameof(RuntimeStatus));
        OnPropertyChanged(nameof(RuntimeStateDisplay));
        OnPropertyChanged(nameof(RuntimeModeDisplay));
        OnPropertyChanged(nameof(RecoveryPlan));
        OnPropertyChanged(nameof(HasRecoveryPlan));
        OnPropertyChanged(nameof(RecoveryStateText));
        OnPropertyChanged(nameof(RecoverySeverityText));
        OnPropertyChanged(nameof(RecoverySummaryText));
        OnPropertyChanged(nameof(RecoveryRequiredActionText));
        OnPropertyChanged(nameof(RecoveryFailedLineNumber));
        OnPropertyChanged(nameof(RecoveryFailedCommandText));
        OnPropertyChanged(nameof(HasRecoveryFailedLine));
        OnPropertyChanged(nameof(HasRecoveryControllerMessage));
        OnPropertyChanged(nameof(RecoveryControllerMessageText));
        OnPropertyChanged(nameof(CanRecoveryRefreshStatus));
        OnPropertyChanged(nameof(CanRecoveryUnlock));
        OnPropertyChanged(nameof(CanRecoveryReconnect));
        OnPropertyChanged(nameof(CanRecoveryClearAlarm));
        OnPropertyChanged(nameof(CanRecoveryHome));
        OnPropertyChanged(nameof(CanRecoveryClearJob));
        OnPropertyChanged(nameof(CanRecoveryResume));
        OnPropertyChanged(nameof(CanRecoveryRestart));
        OnPropertyChanged(nameof(CanRecoveryAbort));
        OnPropertyChanged(nameof(CanRecoveryResetWorkOffset));
        OnPropertyChanged(nameof(ReferenceStatusText));
        OnPropertyChanged(nameof(ReferenceWarningText));
        OnPropertyChanged(nameof(HasReferenceWarning));
        OnPropertyChanged(nameof(HasRuntimeAlarmBanner));
        OnPropertyChanged(nameof(RuntimeAlarmBannerText));
        OnPropertyChanged(nameof(HasRuntimeLockBanner));
        OnPropertyChanged(nameof(RuntimeLockBannerText));
        OnPropertyChanged(nameof(HasRuntimeBlockingReason));
        OnPropertyChanged(nameof(RuntimeBlockingReasonText));
        OnPropertyChanged(nameof(RuntimeFirmwareNameText));
        OnPropertyChanged(nameof(RuntimeFirmwareVersionText));
        OnPropertyChanged(nameof(RuntimeProtocolVersionText));
        OnPropertyChanged(nameof(RuntimeFirmwareConfidenceText));
        OnPropertyChanged(nameof(RuntimeCompatibilityStatusText));
        OnPropertyChanged(nameof(RuntimeCompatibilitySummaryText));
        OnPropertyChanged(nameof(HasRuntimeCompatibilityNotes));
        OnPropertyChanged(nameof(CanCopyDiagnostics));
        OnPropertyChanged(nameof(RuntimeProgressPercentText));
        OnPropertyChanged(nameof(CanConnectMachine));
        OnPropertyChanged(nameof(CanDisconnectMachine));
        OnPropertyChanged(nameof(CanReconnectForRecovery));
    }

    private void RefreshState()
    {
        SyncWorkflowContext();
        _runtimeCoordinator.Refresh();
        RefreshRuntimeStatus();
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
        OnPropertyChanged(nameof(BundledFirmwareVersion));
        OnPropertyChanged(nameof(BundledFirmwareTargetBoard));
        OnPropertyChanged(nameof(ArduinoFirmwareToolingStatus));
        OnPropertyChanged(nameof(CanUploadArduinoFirmware));
        OnPropertyChanged(nameof(IsUploadingFirmware));
        OnPropertyChanged(nameof(ActiveProfileName));
        OnPropertyChanged(nameof(ActiveProfileDriver));
        OnPropertyChanged(nameof(ActiveProfileBaudRateDisplay));
        OnPropertyChanged(nameof(IsSimulationMode));
        OnPropertyChanged(nameof(IsHardwarePortRequired));
        OnPropertyChanged(nameof(RuntimeModeText));
        OnPropertyChanged(nameof(RuntimeStateDisplay));
        OnPropertyChanged(nameof(RuntimeModeDisplay));
        OnPropertyChanged(nameof(HasRuntimeAlarmBanner));
        OnPropertyChanged(nameof(RuntimeAlarmBannerText));
        OnPropertyChanged(nameof(HasRuntimeLockBanner));
        OnPropertyChanged(nameof(RuntimeLockBannerText));
        OnPropertyChanged(nameof(HasRuntimeBlockingReason));
        OnPropertyChanged(nameof(RuntimeBlockingReasonText));
        OnPropertyChanged(nameof(RuntimeFirmwareNameText));
        OnPropertyChanged(nameof(RuntimeFirmwareVersionText));
        OnPropertyChanged(nameof(RuntimeProtocolVersionText));
        OnPropertyChanged(nameof(RuntimeFirmwareConfidenceText));
        OnPropertyChanged(nameof(RuntimeCompatibilityStatusText));
        OnPropertyChanged(nameof(RuntimeCompatibilitySummaryText));
        OnPropertyChanged(nameof(HasRuntimeCompatibilityNotes));
        OnPropertyChanged(nameof(CanCopyDiagnostics));
        OnPropertyChanged(nameof(RuntimeProgressPercentText));
        OnPropertyChanged(nameof(LoadedJobInfo));
        OnPropertyChanged(nameof(HasLoadedJobInfo));
        OnPropertyChanged(nameof(LoadedJobTitle));
        OnPropertyChanged(nameof(LoadedJobSourceReference));
        OnPropertyChanged(nameof(LoadedJobPath));
        OnPropertyChanged(nameof(LoadedJobMotionCount));
        OnPropertyChanged(nameof(LoadedJobProfileName));
        OnPropertyChanged(nameof(LoadedJobDriverMode));
        OnPropertyChanged(nameof(ActiveGcodeUnits));
        OnPropertyChanged(nameof(ActiveGcodeDistanceMode));
        OnPropertyChanged(nameof(ActiveGcodePlane));
        OnPropertyChanged(nameof(UnsupportedCommandCount));
        OnPropertyChanged(nameof(InterpretationSummary));
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
        OnPropertyChanged(nameof(HasUnlockCapability));
        OnPropertyChanged(nameof(HasMotorEnableCapability));
        OnPropertyChanged(nameof(HasMotorDisableCapability));
        OnPropertyChanged(nameof(ShowEnableOrUnlockControl));
        OnPropertyChanged(nameof(ShowDisableMotorsControl));
        OnPropertyChanged(nameof(ShowStatusControl));
        OnPropertyChanged(nameof(EnableOrUnlockButtonText));
        OnPropertyChanged(nameof(CanEnableOrUnlock));
        OnPropertyChanged(nameof(CanEnableMotors));
        OnPropertyChanged(nameof(CanDisableMotors));
        OnPropertyChanged(nameof(CanHome));
        OnPropertyChanged(nameof(CanGoToCenter));
        OnPropertyChanged(nameof(CanSetZero));
        OnPropertyChanged(nameof(CanClearWorkZero));
        OnPropertyChanged(nameof(CanRefreshStatus));
        OnPropertyChanged(nameof(CanLoadGcode));
        OnPropertyChanged(nameof(CanClearLoadedProgram));
        OnPropertyChanged(nameof(HasToolpathPreviewCapability));
        OnPropertyChanged(nameof(HasMachineVisualizationCapability));
        OnPropertyChanged(nameof(HasProgressTrackingCapability));
        OnPropertyChanged(nameof(HasLiveReportedPositionCapability));
        OnPropertyChanged(nameof(MachineX));
        OnPropertyChanged(nameof(MachineY));
        OnPropertyChanged(nameof(MachineZ));
        OnPropertyChanged(nameof(WorkX));
        OnPropertyChanged(nameof(WorkY));
        OnPropertyChanged(nameof(WorkZ));
        OnPropertyChanged(nameof(WorkOffsetX));
        OnPropertyChanged(nameof(WorkOffsetY));
        OnPropertyChanged(nameof(WorkOffsetZ));
        OnPropertyChanged(nameof(PlacementOffsetZ));
        OnPropertyChanged(nameof(AppliedPlacementOffsetZ));
        OnPropertyChanged(nameof(ReferenceStatusText));
        OnPropertyChanged(nameof(ReferenceWarningText));
        OnPropertyChanged(nameof(HasReferenceWarning));
        OnPropertyChanged(nameof(MachineStateValue));
        OnPropertyChanged(nameof(CurrentMotionIndex));
        OnPropertyChanged(nameof(CurrentLineNumber));
        OnPropertyChanged(nameof(CurrentRawLine));
        OnPropertyChanged(nameof(CurrentStreamedCommand));
        OnPropertyChanged(nameof(StreamingStateDisplay));
        OnPropertyChanged(nameof(FailedLineNumber));
        OnPropertyChanged(nameof(FailedStreamedCommand));
        OnPropertyChanged(nameof(CompletedMotionCount));
        OnPropertyChanged(nameof(RemainingMotionCount));
        OnPropertyChanged(nameof(ProgressSummary));
        OnPropertyChanged(nameof(StreamingProgressSummary));
        OnPropertyChanged(nameof(CanUploadArduinoFirmware));
        OnPropertyChanged(nameof(ArduinoFirmwareToolingStatus));
        OnPropertyChanged(nameof(CanStartProgram));
        OnPropertyChanged(nameof(CanPauseProgram));
        OnPropertyChanged(nameof(CanResumeProgram));
        OnPropertyChanged(nameof(CanStopExecution));
        OnPropertyChanged(nameof(CanRestartJob));
        OnPropertyChanged(nameof(CanAbortJob));
        OnPropertyChanged(nameof(BundledFirmwareVersion));
        OnPropertyChanged(nameof(BundledFirmwareTargetBoard));
        OnPropertyChanged(nameof(ArduinoFirmwareToolingStatus));
        OnPropertyChanged(nameof(CanUploadArduinoFirmware));
        OnPropertyChanged(nameof(IsUploadingFirmware));
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
        OnPropertyChanged(nameof(StreamingStateDisplay));
        OnPropertyChanged(nameof(CurrentLineNumber));
        OnPropertyChanged(nameof(CurrentRawLine));
        OnPropertyChanged(nameof(CurrentStreamedCommand));
        OnPropertyChanged(nameof(FailedLineNumber));
        OnPropertyChanged(nameof(FailedStreamedCommand));
        OnPropertyChanged(nameof(CompletedMotionCount));
        OnPropertyChanged(nameof(RemainingMotionCount));
        OnPropertyChanged(nameof(ProgressSummary));
        OnPropertyChanged(nameof(StreamingProgressSummary));
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
            CncExecutionState.JobLoaded => "Job Loaded",
            CncExecutionState.PreflightChecking => "Preflight",
            CncExecutionState.ReadyToRun => "Ready To Run",
            CncExecutionState.Running => "Running",
            CncExecutionState.Paused => "Paused",
            CncExecutionState.Stopping => "Stopping",
            CncExecutionState.Stopped => "Stopped",
            CncExecutionState.Alarmed => "Alarmed",
            CncExecutionState.Error => "Error",
            CncExecutionState.Failed => "Failed",
            CncExecutionState.Completed => "Completed",
            _ => state.ToString()
        };
    }

    private static string FormatStreamingState(CncStreamingState state)
    {
        return state switch
        {
            CncStreamingState.Idle => "Idle",
            CncStreamingState.Planning => "Planning",
            CncStreamingState.Preflight => "Preflight",
            CncStreamingState.Streaming => "Streaming",
            CncStreamingState.Paused => "Paused",
            CncStreamingState.FeedHold => "Feed Hold",
            CncStreamingState.Stopping => "Stopping",
            CncStreamingState.Alarmed => "Alarmed",
            CncStreamingState.Completed => "Completed",
            CncStreamingState.Failed => "Failed",
            CncStreamingState.Cancelled => "Cancelled",
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

public sealed class ExistingMachineCard : ViewModelBase
{
    private bool _isSelected;

    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string ProfileText { get; set; } = string.Empty;
    public string ModeText { get; set; } = string.Empty;
    public string DriverText { get; set; } = string.Empty;
    public string StatusText { get; set; } = string.Empty;
    public RuntimeProfile? RuntimeProfile { get; set; }

    public bool IsSelected
    {
        get => _isSelected;
        set { if (_isSelected == value) return; _isSelected = value; OnPropertyChanged(); }
    }
}
