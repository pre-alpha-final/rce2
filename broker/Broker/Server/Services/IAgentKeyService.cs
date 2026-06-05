namespace Broker.Server.Services;

public interface IAgentKeyService
{
    Task<bool> Validate(Guid agentId, string? agentKey);
    Dictionary<Guid, string> GetAll();
    void SetAll(Dictionary<Guid, string> agentKeys);
}
