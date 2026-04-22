using System.Windows.Input;
using MabaControlCenter.Services;

namespace MabaControlCenter.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly INavigationService _navigation;
    private readonly IAuthSessionService _authSessionService;

    public MainViewModel(INavigationService navigation, IAuthSessionService authSessionService)
    {
        _navigation = navigation;
        _authSessionService = authSessionService;
        _navigation.CurrentViewModelChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(CurrentViewModel));
            OnPropertyChanged(nameof(IsDexterCalibrationModule));
            OnPropertyChanged(nameof(IsLoginPage));
        };
        _authSessionService.AuthenticationChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(IsAuthenticated));
            OnPropertyChanged(nameof(CurrentUserDisplayName));
            if (!_authSessionService.IsAuthenticated)
                _navigation.NavigateTo("Login");
        };
        NavigateCommand = new RelayCommand(Navigate);
        LogoutCommand = new RelayCommand(_ => _authSessionService.Logout());
        _navigation.NavigateTo(_authSessionService.IsAuthenticated ? "Home" : "Login");
    }

    public object? CurrentViewModel => _navigation.CurrentViewModel;
    public bool IsAuthenticated => _authSessionService.IsAuthenticated;
    public bool IsLoginPage => CurrentViewModel is LoginViewModel;
    public string CurrentUserDisplayName => _authSessionService.CurrentUser?.FullName ?? _authSessionService.CurrentUser?.Email ?? string.Empty;

    /// <summary>True when the main content is the Dexter Calibration module (show back bar).</summary>
    public bool IsDexterCalibrationModule => CurrentViewModel is DexterCalibrationViewModel;

    public ICommand NavigateCommand { get; }
    public ICommand LogoutCommand { get; }

    private void Navigate(object? parameter)
    {
        if (parameter is string pageKey)
        {
            if (!_authSessionService.IsAuthenticated && pageKey != "Login")
            {
                _navigation.NavigateTo("Login");
                return;
            }

            _navigation.NavigateTo(pageKey);
        }
    }
}
