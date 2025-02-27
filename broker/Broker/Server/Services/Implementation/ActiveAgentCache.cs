using Broker.Server.Infrastructure;
using Broker.Shared.Model;
using System.Collections.Concurrent;

namespace Broker.Server.Services.Implementation;

public class ActiveAgentCache : IActiveAgentCache
{
    private ConcurrentDictionary<Guid, Agent> _activeAgentCache = new();

    public ActiveAgentCache()
    {
        PubSub.Hub.Default.Subscribe<WhoIsReceived>(this, OnWhoIsReceived);
    }

    public List<Agent> GetMatchingChannelAgents(Guid id, string contact)
    {
        try
        {
            var channels = _activeAgentCache.First(e => e.Key == id)
                .Value
                .Channels
                .Where(e => e != null)
                .ToList();

            return _activeAgentCache.Values
                .Where(e => e.Channels != null)
                .Where(e => e.Channels.Intersect(channels).Any())
                .Where(e => e.Ins.Keys.Contains(contact))
                .ToList();
        }
        catch
        {
        }

        return new List<Agent>();
    }

    public void Remove(Guid agentId)
    {
        _activeAgentCache.TryRemove(agentId, out _);
    }

    private void OnWhoIsReceived(WhoIsReceived whoIsReceived)
    {
        _activeAgentCache.AddOrUpdate(whoIsReceived.Agent.Id, whoIsReceived.Agent, (_, _) => whoIsReceived.Agent);
    }
}
