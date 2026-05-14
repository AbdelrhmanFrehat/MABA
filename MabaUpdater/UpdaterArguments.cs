using System.IO;

namespace MabaUpdater;

public sealed class UpdaterArguments
{
    public required int ProcessId { get; init; }
    public required string SourceDirectory { get; init; }
    public required string TargetDirectory { get; init; }
    public required string RelaunchPath { get; init; }
    public string Version { get; init; } = string.Empty;

    public static UpdaterArguments Parse(string[] args)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < args.Length; i++)
        {
            var token = args[i];
            if (!token.StartsWith("--", StringComparison.Ordinal))
                continue;

            var key = token[2..];
            if (i + 1 >= args.Length)
                throw new InvalidOperationException($"Missing value for argument '{token}'.");

            values[key] = args[++i];
        }

        if (!values.TryGetValue("pid", out var pidText) || !int.TryParse(pidText, out var pid) || pid <= 0)
            throw new InvalidOperationException("Updater requires a valid --pid argument.");

        if (!values.TryGetValue("source", out var source) || string.IsNullOrWhiteSpace(source))
            throw new InvalidOperationException("Updater requires a valid --source argument.");

        if (!values.TryGetValue("target", out var target) || string.IsNullOrWhiteSpace(target))
            throw new InvalidOperationException("Updater requires a valid --target argument.");

        if (!values.TryGetValue("relaunch", out var relaunch) || string.IsNullOrWhiteSpace(relaunch))
            throw new InvalidOperationException("Updater requires a valid --relaunch argument.");

        values.TryGetValue("version", out var version);

        return new UpdaterArguments
        {
            ProcessId = pid,
            SourceDirectory = Path.GetFullPath(source),
            TargetDirectory = Path.GetFullPath(target),
            RelaunchPath = Path.GetFullPath(relaunch),
            Version = version?.Trim() ?? string.Empty
        };
    }
}
