using Newtonsoft.Json;

namespace BackupRestoreAgent.Config;

/// <summary>
/// Loads and persists <see cref="AgentConfig"/> as JSON in the executable's folder,
/// so the configured path survives between runs.
/// </summary>
public class ConfigRepository
{
    private static readonly string ConfigPath =
        Path.Combine(AppContext.BaseDirectory, "config.json");

    public AgentConfig Load()
    {
        if (File.Exists(ConfigPath) == false)
        {
            return new AgentConfig();
        }

        var json = File.ReadAllText(ConfigPath);
        return JsonConvert.DeserializeObject<AgentConfig>(json) ?? new AgentConfig();
    }

    public void Save(AgentConfig config)
    {
        var json = JsonConvert.SerializeObject(config, Formatting.Indented);
        File.WriteAllText(ConfigPath, json);
    }
}
