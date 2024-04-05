using Broker.Shared.Model;

namespace Broker.Server.Services;

public interface IActiveAgentCache
{
    List<Agent> GetMatchingChannelAgents(Guid id, string contact);
    void Remove(Guid agentId);
}
