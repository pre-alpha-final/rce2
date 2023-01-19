using Broker.Shared.Infrastructure;
using System.Collections.Concurrent;

namespace Broker.Server.Services.Implementation;

public class FeedRepository : IFeedRepository
{
    private readonly ConcurrentDictionary<Guid, ConcurrentQueue<Rce2Message>> _feeds = new();

    public bool Exists(Guid id)
    {
        return _feeds.ContainsKey(id);
    }

    public void AddItem(Guid id, Rce2Message rce2Message)
    {
        var feed = _feeds.GetOrAdd(id, new ConcurrentQueue<Rce2Message>());
        feed.Enqueue(rce2Message);
    }

    public Rce2Message GetNext(Guid id)
    {
        var feed = _feeds.GetOrAdd(id, new ConcurrentQueue<Rce2Message>());
        feed.TryDequeue(out var next);
        return next;
    }
}
