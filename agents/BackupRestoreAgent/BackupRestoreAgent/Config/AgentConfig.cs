namespace BackupRestoreAgent.Config;

public class AgentConfig
{
    /// <summary>
    /// The folder that gets backed up / restored. Remembered between runs.
    /// </summary>
    public string? Path { get; set; }

    /// <summary>
    /// Optional override for the full path to 7z.exe. When null, common
    /// install locations and PATH are probed automatically.
    /// </summary>
    public string? SevenZipPath { get; set; }
}
