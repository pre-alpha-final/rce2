using Broker.Shared.Events;
using System.Collections.Concurrent;

namespace Broker.Server.Services.Implementation;

public class BrokerFeedRepository : IBrokerFeedRepository
{
    private readonly ConcurrentDictionary<Guid, ConcurrentQueue<BrokerEventBase>> _feeds = new();
    private readonly IRecentMessagesRepository _recentMessagesRepository;

    public BrokerFeedRepository(IRecentMessagesRepository recentMessagesRepository)
    {
        _recentMessagesRepository = recentMessagesRepository;
    }

    public bool Exists(Guid feedId)
    {
        return _feeds.ContainsKey(feedId);
    }

    public void AddItem(Guid feedId, BrokerEventBase item)
    {
        var feed = _feeds.GetOrAdd(feedId, new ConcurrentQueue<BrokerEventBase>());
        feed.Enqueue(item);
        _recentMessagesRepository.AddItem(item);
    }

    public void AddItems(Guid feedId, IEnumerable<BrokerEventBase> items)
    {
        foreach (var item in items)
        {
            AddItem(feedId, item);
        }
    }

    public void BroadcastItem(BrokerEventBase item)
    {
        var feeds = _feeds.ToList();
        foreach (var feed in feeds)
        {
            feed.Value.Enqueue(item);
        }
        _recentMessagesRepository.AddItem(item);
    }

    public BrokerEventBase GetNext(Guid feedId)
    {
        var feed = _feeds.GetOrAdd(feedId, new ConcurrentQueue<BrokerEventBase>());
        feed.TryDequeue(out var next);

        return next;
    }

    public bool Delete(Guid feedId)
    {
        return _feeds.TryRemove(feedId, out var feed);
    }
}
