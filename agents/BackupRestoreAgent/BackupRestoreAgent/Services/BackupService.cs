using System.Diagnostics;
using System.Text;
using BackupRestoreAgent.Config;

namespace BackupRestoreAgent.Services;

/// <summary>
/// Creates and restores timestamped 7-zip archives of a folder by shelling out to 7z.exe.
/// Archives live in a "backups" folder next to the executable and are named
/// "{sanitizedFolderName}__{yyyyMMdd-HHmmss}.7z" so they can be scoped to a source path
/// and sorted by their embedded timestamp.
/// </summary>
public class BackupService
{
    private const string TimestampFormat = "yyyyMMdd-HHmmss";

    private static readonly string BackupsDirectory =
        Path.Combine(AppContext.BaseDirectory, "backups");

    private readonly ConfigRepository _configRepository;

    public BackupService(ConfigRepository configRepository)
    {
        _configRepository = configRepository;
    }

    public string Backup(string sourcePath)
    {
        if (string.IsNullOrWhiteSpace(sourcePath))
        {
            return "backup failed: no path is set (use set-path first)";
        }

        if (Directory.Exists(sourcePath) == false)
        {
            return $"backup failed: directory does not exist: '{sourcePath}'";
        }

        Directory.CreateDirectory(BackupsDirectory);

        var timestamp = DateTime.Now.ToString(TimestampFormat);
        var archiveName = $"{SanitizedFolderName(sourcePath)}__{timestamp}.7z";
        var archivePath = Path.Combine(BackupsDirectory, archiveName);

        // "a" = add to archive; archive the folder itself so restore recreates it.
        var result = RunSevenZip($"a \"{archivePath}\" \"{sourcePath.TrimEnd('\\', '/')}\"");
        if (result.ExitCode != 0)
        {
            return $"backup failed (7z exit {result.ExitCode}): {result.Output}";
        }

        return $"backup created: {archiveName}";
    }

    public string Restore(string sourcePath)
    {
        if (string.IsNullOrWhiteSpace(sourcePath))
        {
            return "restore failed: no path is set (use set-path first)";
        }

        var latest = FindLatestBackup(sourcePath);
        if (latest == null)
        {
            return $"restore aborted: no backup found for '{sourcePath}'";
        }

        // Safety gate: only remove the existing folder once a backup is confirmed.
        if (Directory.Exists(sourcePath))
        {
            Directory.Delete(sourcePath, recursive: true);
        }

        var parent = Path.GetDirectoryName(sourcePath.TrimEnd('\\', '/'));
        if (string.IsNullOrEmpty(parent))
        {
            return $"restore failed: cannot determine parent directory of '{sourcePath}'";
        }

        Directory.CreateDirectory(parent);

        // "x" = extract with full paths; -o sets output dir; -y assumes yes to prompts.
        var result = RunSevenZip($"x \"{latest}\" -o\"{parent}\" -y");
        if (result.ExitCode != 0)
        {
            return $"restore failed (7z exit {result.ExitCode}): {result.Output}";
        }

        return $"restored '{sourcePath}' from {Path.GetFileName(latest)}";
    }

    /// <summary>
    /// Finds the newest backup archive for the given source path by parsing the
    /// timestamp embedded in each matching archive's file name.
    /// </summary>
    private static string? FindLatestBackup(string sourcePath)
    {
        if (Directory.Exists(BackupsDirectory) == false)
        {
            return null;
        }

        var prefix = $"{SanitizedFolderName(sourcePath)}__";

        return Directory.EnumerateFiles(BackupsDirectory, "*.7z")
            .Select(path => new { Path = path, Name = Path.GetFileNameWithoutExtension(path) })
            .Where(x => x.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .Select(x => new { x.Path, Timestamp = ParseTimestamp(x.Name, prefix) })
            .Where(x => x.Timestamp != null)
            .OrderByDescending(x => x.Timestamp!.Value)
            .Select(x => x.Path)
            .FirstOrDefault();
    }

    private static DateTime? ParseTimestamp(string fileNameWithoutExtension, string prefix)
    {
        var stamp = fileNameWithoutExtension.Substring(prefix.Length);
        return DateTime.TryParseExact(stamp, TimestampFormat, null,
            System.Globalization.DateTimeStyles.None, out var parsed)
            ? parsed
            : null;
    }

    private static string SanitizedFolderName(string sourcePath)
    {
        var leaf = Path.GetFileName(sourcePath.TrimEnd('\\', '/'));
        if (string.IsNullOrEmpty(leaf))
        {
            // e.g. a drive root like "C:\" — fall back to a stable token.
            leaf = sourcePath.Replace(":", string.Empty).Replace("\\", string.Empty).Replace("/", string.Empty);
        }

        var sanitized = new StringBuilder(leaf.Length);
        foreach (var c in leaf)
        {
            sanitized.Append(Array.IndexOf(Path.GetInvalidFileNameChars(), c) >= 0 ? '_' : c);
        }

        return sanitized.ToString();
    }

    private (int ExitCode, string Output) RunSevenZip(string arguments)
    {
        var exe = ResolveSevenZip();

        var startInfo = new ProcessStartInfo
        {
            FileName = exe,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException($"failed to start 7-zip process '{exe}'");

        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();

        var output = string.Join(" | ",
            new[] { stdout, stderr }
                .Select(s => s.Trim())
                .Where(s => string.IsNullOrEmpty(s) == false));

        return (process.ExitCode, output);
    }

    /// <summary>
    /// Resolves the 7z executable: explicit config override, then common install
    /// locations, then a bare "7z" assumed to be on PATH.
    /// </summary>
    private string ResolveSevenZip()
    {
        var configured = _configRepository.Load().SevenZipPath;
        if (string.IsNullOrWhiteSpace(configured) == false)
        {
            if (File.Exists(configured) == false)
            {
                throw new FileNotFoundException(
                    $"configured SevenZipPath does not exist: '{configured}'");
            }

            return configured;
        }

        var candidates = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "7-Zip", "7z.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "7-Zip", "7z.exe"),
        };

        foreach (var candidate in candidates)
        {
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        // Last resort: rely on PATH. If 7-Zip isn't installed, Process.Start throws,
        // which is surfaced to the "out" channel by the caller.
        return "7z";
    }
}
