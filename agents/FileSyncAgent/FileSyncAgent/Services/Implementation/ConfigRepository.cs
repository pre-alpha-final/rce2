using FileSyncAgent.Infrastructure;
using Newtonsoft.Json;

namespace FileSyncAgent.Services.Implementation;

public class ConfigRepository : IConfigRepository
{
    private const string ConfigFileName = "FileSync.config";

    public async Task<FileSyncConfig> Load()
    {
        if (File.Exists("FileSync.config") == false)
        {
            throw new Exception("No config file found");
        }

        return JsonConvert.DeserializeObject<FileSyncConfig>(await File.ReadAllTextAsync(ConfigFileName));
    }

    public async Task Save(FileSyncConfig fileSyncConfig)
    {
        if (File.Exists("FileSync.config") == false)
        {
            throw new Exception("No config file found");
        }

        fileSyncConfig.Syncs = fileSyncConfig.SyncsDescending.Take(100).ToList();

        await File.WriteAllTextAsync(ConfigFileName, JsonConvert.SerializeObject(fileSyncConfig));
    }
}
