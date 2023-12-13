using Broker.Shared.Model;
using System.Collections.Concurrent;

namespace Broker.Server.Services.Implementation;

public class AgentFeedRepository : IAgentFeedRepository
{
    private readonly ConcurrentDictionary<Guid, ConcurrentQueue<Rce2Message>> _feeds = new();

    public bool Exists(Guid feedId)
    {
        return _feeds.ContainsKey(feedId);
    }

    public void AddItem(Guid feedId, Rce2Message item)
    {
        var feed = _feeds.GetOrAdd(feedId, new ConcurrentQueue<Rce2Message>());
        feed.Enqueue(item);
    }

    public void AddItems(Guid feedId, IEnumerable<Rce2Message> items)
    {
        foreach (var item in items)
        {
            AddItem(feedId, item);
        }
    }

    public void BroadcastItem(Rce2Message item)
    {
        var feeds = _feeds.ToList();
        foreach (var feed in feeds)
        {
            feed.Value.Enqueue(item);
        }
    }

    public Rce2Message GetNext(Guid feedId)
    {
        var feed = _feeds.GetOrAdd(feedId, new ConcurrentQueue<Rce2Message>());
        feed.TryDequeue(out var next);

        return next;
    }

    public bool Delete(Guid feedId)
    {
        return _feeds.TryRemove(feedId, out var feed);
    }
}
