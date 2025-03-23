namespace Broker.Server.Services;

public interface IAgentKeyService
{
    Task<bool> Validate(Guid agentId, string? agentKey);
}
