namespace Broker.Server.Services;

public class AgentKeyService : IAgentKeyService
{
    private Dictionary<Guid, string> _agentKeys { get; set; } = new();

    public async Task<bool> Validate(Guid agentId, string? agentKey)
    {
        if (_agentKeys.ContainsKey(agentId))
        {
            return agentKey == _agentKeys[agentId];
        }

        return true;
    }
}
