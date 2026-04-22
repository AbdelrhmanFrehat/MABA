using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public class AuthSessionService : IAuthSessionService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private readonly ISettingsService _settingsService;
    private readonly string _sessionFilePath;
    private AuthSession? _session;

    public AuthSessionService(ISettingsService settingsService)
    {
        _settingsService = settingsService;
        var baseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MabaControlCenter");
        _sessionFilePath = Path.Combine(baseDir, "auth-session.json");
        LoadSession();
    }

    public bool IsAuthenticated => _session?.User != null
        && !string.IsNullOrWhiteSpace(_session.Token)
        && _session.ExpiresAt > DateTime.UtcNow.AddMinutes(1);

    public AuthUser? CurrentUser => IsAuthenticated ? _session?.User : null;
    public AuthSession? CurrentSession => IsAuthenticated ? _session : null;
    public event EventHandler? AuthenticationChanged;

    public async Task<bool> LoginAsync(string email, string password, bool rememberMe, CancellationToken cancellationToken = default)
    {
        using var client = new HttpClient();
        var baseUrl = _settingsService.Load().ApiBaseUrl?.TrimEnd('/') ?? "https://api.mabasol.com";
        var payload = JsonSerializer.Serialize(new { email, password }, JsonOptions);
        using var content = new StringContent(payload, Encoding.UTF8, "application/json");
        using var response = await client.PostAsync($"{baseUrl}/api/v1/auth/login", content, cancellationToken);

        if (!response.IsSuccessStatusCode)
            return false;

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var login = await JsonSerializer.DeserializeAsync<AuthLoginResponse>(stream, JsonOptions, cancellationToken);
        if (login == null || string.IsNullOrWhiteSpace(login.Token))
            return false;

        _session = new AuthSession
        {
            Token = login.Token,
            RefreshToken = login.RefreshToken,
            ExpiresAt = login.ExpiresAt,
            User = login.User
        };

        if (rememberMe)
            SaveSession();
        else
            DeletePersistedSession();

        AuthenticationChanged?.Invoke(this, EventArgs.Empty);
        return true;
    }

    public void Logout()
    {
        _session = null;
        DeletePersistedSession();

        AuthenticationChanged?.Invoke(this, EventArgs.Empty);
    }

    public HttpClient CreateHttpClient()
    {
        var client = new HttpClient();
        if (IsAuthenticated && !string.IsNullOrWhiteSpace(_session?.Token))
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _session.Token);
        return client;
    }

    private void LoadSession()
    {
        try
        {
            if (!File.Exists(_sessionFilePath))
                return;

            var json = File.ReadAllText(_sessionFilePath);
            _session = JsonSerializer.Deserialize<AuthSession>(json, JsonOptions);
            if (!IsAuthenticated)
                _session = null;
        }
        catch
        {
            _session = null;
        }
    }

    private void SaveSession()
    {
        try
        {
            var dir = Path.GetDirectoryName(_sessionFilePath);
            if (!string.IsNullOrWhiteSpace(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllText(_sessionFilePath, JsonSerializer.Serialize(_session, JsonOptions));
        }
        catch
        {
            // The login still works for the current run even if persistence fails.
        }
    }

    private void DeletePersistedSession()
    {
        try
        {
            if (File.Exists(_sessionFilePath))
                File.Delete(_sessionFilePath);
        }
        catch
        {
            // Local session cleanup should not crash the app.
        }
    }
}
