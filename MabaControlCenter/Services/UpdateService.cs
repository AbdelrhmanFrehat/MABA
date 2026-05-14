using System.Diagnostics;
using System.IO;
using System.Globalization;
using System.IO.Compression;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public class UpdateService : IUpdateService
{
    private readonly ISettingsService _settingsService;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };
    private AppUpdateInfo _info;
    private AppReleaseManifest? _latestManifest;
    private string? _resolvedPackageUri;

    public UpdateService(ISettingsService settingsService)
    {
        _settingsService = settingsService;
        _info = new AppUpdateInfo
        {
            CurrentVersion = GetCurrentVersion(),
            LatestVersion = GetCurrentVersion(),
            StatusMessage = "Update source not configured."
        };
    }

    public AppUpdateInfo GetUpdateInfo() => _info;

    public event EventHandler? UpdateInfoChanged;

    public async Task CheckForUpdatesAsync(bool userInitiated = true, CancellationToken cancellationToken = default)
    {
        var settings = _settingsService.Load();
        var source = settings.UpdateManifestUri?.Trim();
        var currentVersion = GetCurrentVersion();

        if (string.IsNullOrWhiteSpace(source))
        {
            UpdateInfo(info =>
            {
                info.CurrentVersion = currentVersion;
                info.LatestVersion = currentVersion;
                info.IsUpdateAvailable = false;
                info.CanInstall = false;
                info.IsBusy = false;
                info.UpdateSource = string.Empty;
                info.ReleaseNotes = string.Empty;
                info.StatusMessage = userInitiated
                    ? "Set an update manifest path or URL in Settings first."
                    : "Update source not configured.";
            });
            return;
        }

        UpdateInfo(info =>
        {
            info.CurrentVersion = currentVersion;
            info.IsBusy = true;
            info.UpdateSource = source;
            info.StatusMessage = "Checking for updates...";
        });

        try
        {
            var (manifest, resolvedPackageUri) = await LoadManifestAsync(source, cancellationToken);
            _latestManifest = manifest;
            _resolvedPackageUri = resolvedPackageUri;

            var isUpdateAvailable = IsNewerVersion(manifest.Version, currentVersion);
            UpdateInfo(info =>
            {
                info.CurrentVersion = currentVersion;
                info.LatestVersion = string.IsNullOrWhiteSpace(manifest.Version) ? currentVersion : manifest.Version;
                info.IsUpdateAvailable = isUpdateAvailable;
                info.CanInstall = isUpdateAvailable && !string.IsNullOrWhiteSpace(_resolvedPackageUri);
                info.ReleaseNotes = manifest.Notes ?? string.Empty;
                info.StatusMessage = isUpdateAvailable
                    ? "Update available."
                    : "You are up to date.";
                info.IsBusy = false;
            });
        }
        catch (Exception ex)
        {
            UpdateInfo(info =>
            {
                info.CurrentVersion = currentVersion;
                info.LatestVersion = currentVersion;
                info.IsUpdateAvailable = false;
                info.CanInstall = false;
                info.IsBusy = false;
                info.ReleaseNotes = string.Empty;
                info.StatusMessage = $"Update check failed: {ex.Message}";
            });
        }
    }

    public async Task<bool> InstallUpdateAsync(CancellationToken cancellationToken = default)
    {
        if (_latestManifest == null || string.IsNullOrWhiteSpace(_resolvedPackageUri) || !_info.CanInstall)
        {
            UpdateInfo(info => info.StatusMessage = "No installable update is ready.");
            return false;
        }

        UpdateInfo(info =>
        {
            info.IsBusy = true;
            info.StatusMessage = "Preparing update package...";
        });

        try
        {
            var updateRoot = Path.Combine(Path.GetTempPath(), "MabaControlCenter", "updates", Guid.NewGuid().ToString("N"));
            var zipPath = Path.Combine(updateRoot, "release.zip");
            var extractRoot = Path.Combine(updateRoot, "extract");
            Directory.CreateDirectory(updateRoot);

            await DownloadPackageAsync(_resolvedPackageUri, zipPath, cancellationToken);
            Directory.CreateDirectory(extractRoot);
            ZipFile.ExtractToDirectory(zipPath, extractRoot, overwriteFiles: true);

            var currentExePath = Environment.ProcessPath ?? throw new InvalidOperationException("Current executable path is unavailable.");
            var installDirectory = Path.GetDirectoryName(currentExePath) ?? throw new InvalidOperationException("Install directory is unavailable.");
            var exeName = Path.GetFileName(currentExePath);
            var extractedAppDirectory = FindExtractedAppDirectory(extractRoot, exeName);
            var relaunchPath = Path.Combine(installDirectory, exeName);
            var launch = PrepareUpdaterLaunch(updateRoot, installDirectory, extractedAppDirectory, relaunchPath, _latestManifest.Version);

            UpdateInfo(info => info.StatusMessage = launch.UsingNativeUpdater
                ? "Launching native updater and restarting..."
                : "Installing update, closing the app, and restarting...");

            Process.Start(launch.StartInfo);
            Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
            {
                App.RestartRequested = false;
                Application.Current?.Shutdown();
            }));
            return true;
        }
        catch (Exception ex)
        {
            UpdateInfo(info =>
            {
                info.IsBusy = false;
                info.StatusMessage = $"Install failed: {ex.Message}";
            });
            return false;
        }
    }

    private async Task<(AppReleaseManifest Manifest, string ResolvedPackageUri)> LoadManifestAsync(string source, CancellationToken cancellationToken)
    {
        string json;
        if (TryCreateHttpUri(source, out var httpUri))
        {
            using var http = new HttpClient();
            json = await http.GetStringAsync(httpUri, cancellationToken);
        }
        else
        {
            var manifestPath = Path.GetFullPath(Environment.ExpandEnvironmentVariables(source));
            if (!File.Exists(manifestPath))
                throw new FileNotFoundException("Update manifest file was not found.", manifestPath);

            json = await File.ReadAllTextAsync(manifestPath, cancellationToken);
        }

        var manifest = JsonSerializer.Deserialize<AppReleaseManifest>(json, _jsonOptions)
            ?? throw new InvalidOperationException("Update manifest could not be parsed.");

        if (string.IsNullOrWhiteSpace(manifest.Version))
            throw new InvalidOperationException("Update manifest is missing a version.");

        if (string.IsNullOrWhiteSpace(manifest.PackageUri))
            throw new InvalidOperationException("Update manifest is missing a package path.");

        var resolvedPackageUri = ResolvePackageUri(source, manifest.PackageUri);
        return (manifest, resolvedPackageUri);
    }

    private async Task DownloadPackageAsync(string packageUri, string destinationPath, CancellationToken cancellationToken)
    {
        if (TryCreateHttpUri(packageUri, out var httpUri))
        {
            using var http = new HttpClient();
            await using var responseStream = await http.GetStreamAsync(httpUri, cancellationToken);
            await using var fileStream = File.Create(destinationPath);
            await responseStream.CopyToAsync(fileStream, cancellationToken);
            return;
        }

        var sourcePath = Path.GetFullPath(Environment.ExpandEnvironmentVariables(packageUri));
        if (!File.Exists(sourcePath))
            throw new FileNotFoundException("Update package file was not found.", sourcePath);

        File.Copy(sourcePath, destinationPath, overwrite: true);
    }

    private static string ResolvePackageUri(string manifestSource, string packageUri)
    {
        if (TryCreateHttpUri(packageUri, out var packageHttpUri))
            return packageHttpUri!.AbsoluteUri;

        if (TryCreateHttpUri(manifestSource, out var manifestHttpUri))
            return new Uri(manifestHttpUri!, packageUri).AbsoluteUri;

        if (Path.IsPathRooted(packageUri))
            return packageUri;

        var manifestPath = Path.GetFullPath(Environment.ExpandEnvironmentVariables(manifestSource));
        var manifestDirectory = Path.GetDirectoryName(manifestPath) ?? AppContext.BaseDirectory;
        return Path.GetFullPath(Path.Combine(manifestDirectory, packageUri));
    }

    private static string FindExtractedAppDirectory(string extractRoot, string exeName)
    {
        var rootCandidate = Path.Combine(extractRoot, exeName);
        if (File.Exists(rootCandidate))
            return extractRoot;

        var nestedCandidate = Directory.EnumerateFiles(extractRoot, exeName, SearchOption.AllDirectories).FirstOrDefault();
        if (nestedCandidate == null)
            throw new FileNotFoundException($"The update package does not contain {exeName}.");

        return Path.GetDirectoryName(nestedCandidate) ?? extractRoot;
    }

    private static PreparedUpdaterLaunch PrepareUpdaterLaunch(
        string updateRoot,
        string installDirectory,
        string extractedAppDirectory,
        string relaunchPath,
        string version)
    {
        var installedHelper = Path.Combine(installDirectory, "Updater", "MabaUpdater.exe");
        if (File.Exists(installedHelper))
        {
            var helperTempDirectory = Path.Combine(updateRoot, "updater");
            CopyDirectory(Path.GetDirectoryName(installedHelper)!, helperTempDirectory);
            var helperExe = Path.Combine(helperTempDirectory, "MabaUpdater.exe");
            var startInfo = new ProcessStartInfo
            {
                FileName = helperExe,
                Arguments = BuildUpdaterArguments(Environment.ProcessId, extractedAppDirectory, installDirectory, relaunchPath, version),
                UseShellExecute = true,
                WorkingDirectory = helperTempDirectory
            };
            return new PreparedUpdaterLaunch(startInfo, true);
        }

        var scriptPath = Path.Combine(updateRoot, "install-update.ps1");
        File.WriteAllText(
            scriptPath,
            BuildInstallerScript(Environment.ProcessId, extractedAppDirectory, installDirectory, relaunchPath),
            Encoding.UTF8);

        var fallback = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-NoProfile -ExecutionPolicy Bypass -WindowStyle Hidden -File \"{scriptPath}\"",
            CreateNoWindow = true,
            UseShellExecute = true,
            WindowStyle = ProcessWindowStyle.Hidden,
            WorkingDirectory = updateRoot
        };
        return new PreparedUpdaterLaunch(fallback, false);
    }

    private static string BuildUpdaterArguments(int pid, string sourceDirectory, string targetDirectory, string relaunchPath, string version)
    {
        return $"--pid {pid} --source \"{sourceDirectory}\" --target \"{targetDirectory}\" --relaunch \"{relaunchPath}\" --version \"{version}\"";
    }

    private static string BuildInstallerScript(int pid, string sourceDirectory, string targetDirectory, string relaunchPath)
    {
        var source = EscapePowerShellLiteral(sourceDirectory);
        var target = EscapePowerShellLiteral(targetDirectory);
        var relaunch = EscapePowerShellLiteral(relaunchPath);

        return $@"$ErrorActionPreference = 'Stop'
$pidToWait = {pid}
$sourceDirectory = '{source}'
$targetDirectory = '{target}'
$relaunchPath = '{relaunch}'

while (Get-Process -Id $pidToWait -ErrorAction SilentlyContinue) {{
    Start-Sleep -Milliseconds 500
}}

$null = New-Item -ItemType Directory -Force -Path $targetDirectory
& robocopy $sourceDirectory $targetDirectory /E /PURGE /R:2 /W:1 /NFL /NDL /NJH /NJS /NP | Out-Null
$robocopyExitCode = $LASTEXITCODE
if ($robocopyExitCode -ge 8) {{
    throw ""robocopy failed with exit code $robocopyExitCode.""
}}

Start-Process -FilePath $relaunchPath -WorkingDirectory (Split-Path -Path $relaunchPath -Parent) -WindowStyle Normal
";
    }

    private static string EscapePowerShellLiteral(string value)
    {
        return value.Replace("'", "''", StringComparison.Ordinal);
    }

    private static void CopyDirectory(string sourceDirectory, string targetDirectory)
    {
        Directory.CreateDirectory(targetDirectory);

        foreach (var directory in Directory.GetDirectories(sourceDirectory, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(sourceDirectory, directory);
            Directory.CreateDirectory(Path.Combine(targetDirectory, relative));
        }

        foreach (var file in Directory.GetFiles(sourceDirectory, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(sourceDirectory, file);
            var targetPath = Path.Combine(targetDirectory, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
            File.Copy(file, targetPath, overwrite: true);
        }
    }

    private void UpdateInfo(Action<AppUpdateInfo> mutate)
    {
        mutate(_info);
        UpdateInfoChanged?.Invoke(this, EventArgs.Empty);
        CommandManager.InvalidateRequerySuggested();
    }

    private static string GetCurrentVersion()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var informational = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (!string.IsNullOrWhiteSpace(informational))
            return informational.Split('+')[0];

        var version = assembly.GetName().Version;
        if (version == null)
            return "0.0.0";

        return version.Build > 0
            ? $"{version.Major}.{version.Minor}.{version.Build}"
            : $"{version.Major}.{version.Minor}.0";
    }

    private static bool IsNewerVersion(string candidate, string current)
    {
        if (Version.TryParse(NormalizeVersion(candidate), out var candidateVersion) &&
            Version.TryParse(NormalizeVersion(current), out var currentVersion))
        {
            return candidateVersion > currentVersion;
        }

        return !string.Equals(candidate, current, StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeVersion(string value)
    {
        var sanitized = value.Trim();
        return sanitized.Count(c => c == '.') switch
        {
            0 => sanitized + ".0.0",
            1 => sanitized + ".0",
            _ => sanitized
        };
    }

    private static bool TryCreateHttpUri(string value, out Uri? uri)
    {
        if (Uri.TryCreate(value, UriKind.Absolute, out var parsed) &&
            (parsed.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) ||
             parsed.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)))
        {
            uri = parsed;
            return true;
        }

        uri = null;
        return false;
    }

    private sealed record PreparedUpdaterLaunch(ProcessStartInfo StartInfo, bool UsingNativeUpdater);
}
