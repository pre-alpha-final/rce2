namespace Broker.Server.Services;

public class AgentKeyService : IAgentKeyService
{
    private readonly IConfiguration _configuration;
    private Dictionary<Guid, string> _agentKeys { get; set; } = new();

    public AgentKeyService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<bool> Validate(Guid agentId, string? agentKey)
    {
        if (_agentKeys.ContainsKey(agentId) && agentKey == _agentKeys[agentId])
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(agentKey) && Convert.ToBoolean(_configuration["ForceAgentAuth"]) == false)
        {
            return true;
        }

        return false;
    }
}
