using System.Windows.Input;
using System.Diagnostics;
using System.Net.Http;
using MabaControlCenter.Services;

namespace MabaControlCenter.ViewModels;

public class LoginViewModel : ViewModelBase
{
    private readonly IAuthSessionService _authSessionService;
    private readonly INavigationService _navigationService;
    private readonly ISettingsService _settingsService;
    private string _email = string.Empty;
    private string _password = string.Empty;
    private string _statusText = "Sign in with your MABA account to continue.";
    private bool _isBusy;
    private bool _rememberMe;

    public LoginViewModel(IAuthSessionService authSessionService, INavigationService navigationService, ISettingsService settingsService)
    {
        _authSessionService = authSessionService;
        _navigationService = navigationService;
        _settingsService = settingsService;
        LoginCommand = new RelayCommand(async _ => await LoginAsync(), _ => !IsBusy);
        ForgotPasswordCommand = new RelayCommand(_ => OpenWebsitePath("/auth/forgot-password"));
        CreateAccountCommand = new RelayCommand(_ => OpenWebsitePath("/auth/register"));
    }

    public string Email
    {
        get => _email;
        set { if (_email == value) return; _email = value; OnPropertyChanged(); }
    }

    public string Password
    {
        get => _password;
        set { if (_password == value) return; _password = value; OnPropertyChanged(); }
    }

    public string StatusText
    {
        get => _statusText;
        set { if (_statusText == value) return; _statusText = value; OnPropertyChanged(); }
    }

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (_isBusy == value) return;
            _isBusy = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SignInButtonText));
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public bool RememberMe
    {
        get => _rememberMe;
        set { if (_rememberMe == value) return; _rememberMe = value; OnPropertyChanged(); }
    }

    public string SignInButtonText => IsBusy ? "Signing in..." : "Sign In";
    public ICommand LoginCommand { get; }
    public ICommand ForgotPasswordCommand { get; }
    public ICommand CreateAccountCommand { get; }

    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            StatusText = "Enter your email and password.";
            return;
        }

        IsBusy = true;
        StatusText = "Signing in...";
        try
        {
            var ok = await _authSessionService.LoginAsync(Email.Trim(), Password, RememberMe);
            if (!ok)
            {
                StatusText = "Invalid email or password. Please try again.";
                return;
            }

            Password = string.Empty;
            StatusText = "Signed in.";
            _navigationService.NavigateTo("Home");
        }
        catch (HttpRequestException)
        {
            StatusText = "Network error. Check the API connection and try again.";
        }
        catch (TaskCanceledException)
        {
            StatusText = "Network timeout. Check the API connection and try again.";
        }
        catch (Exception ex)
        {
            StatusText = $"Sign in failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void OpenWebsitePath(string path)
    {
        var baseUrl = ResolveWebsiteBaseUrl();
        var url = $"{baseUrl.TrimEnd('/')}/{path.TrimStart('/')}";
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }

    private string ResolveWebsiteBaseUrl()
    {
        var apiBase = _settingsService.Load().ApiBaseUrl?.TrimEnd('/');
        if (string.IsNullOrWhiteSpace(apiBase))
            return "https://mabasol.com";

        if (!Uri.TryCreate(apiBase, UriKind.Absolute, out var uri))
            return "https://mabasol.com";

        if (uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) || uri.Host.Equals("127.0.0.1"))
            return $"{uri.Scheme}://localhost:4200";

        var host = uri.Host.StartsWith("api.", StringComparison.OrdinalIgnoreCase)
            ? uri.Host[4..]
            : uri.Host;

        return $"{uri.Scheme}://{host}";
    }
}
