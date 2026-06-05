using Newtonsoft.Json;

namespace Broker.Server.Services;

public class AgentKeyService : IAgentKeyService
{
    private const string FileName = "agentKeys.txt";
    private readonly object _lock = new object();

    private Dictionary<Guid, string> _agentKeys { get; set; } = new();

    public AgentKeyService()
    {
        LoadFromFile();
    }

    public async Task<bool> Validate(Guid agentId, string? agentKey)
    {
        lock (_lock)
        {
            if (_agentKeys.ContainsKey(agentId))
            {
                return agentKey == _agentKeys[agentId];
            }

            return true;
        }
    }

    public Dictionary<Guid, string> GetAll()
    {
        lock (_lock)
        {
            return new Dictionary<Guid, string>(_agentKeys);
        }
    }

    public void SetAll(Dictionary<Guid, string> agentKeys)
    {
        lock (_lock)
        {
            _agentKeys = agentKeys;
            SaveToFile();
        }
    }

    private void LoadFromFile()
    {
        try
        {
            var content = File.ReadAllText(FileName);
            _agentKeys = JsonConvert.DeserializeObject<Dictionary<Guid, string>>(content) ?? new Dictionary<Guid, string>();
        }
        catch (Exception e)
        {
            // ignore
        }
    }

    private void SaveToFile()
    {
        try
        {
            File.WriteAllText(FileName, JsonConvert.SerializeObject(_agentKeys));
        }
        catch (Exception e)
        {
            // ignore
        }
    }
}
