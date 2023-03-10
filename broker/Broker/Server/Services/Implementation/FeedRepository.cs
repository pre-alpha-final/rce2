using System.Collections.Concurrent;

namespace Broker.Server.Services.Implementation;

public class FeedRepository<T> : IFeedRepository<T> where T : class
{
    private readonly ConcurrentDictionary<Guid, ConcurrentQueue<T>> _feeds = new();
    private readonly IEchoFeedRepository<T> _echoFeedRepository;

    public FeedRepository(IEchoFeedRepository<T> echoFeedRepository)
    {
        _echoFeedRepository = echoFeedRepository;
    }

    public bool Exists(Guid id)
    {
        return _feeds.ContainsKey(id);
    }

    public void AddItem(Guid id, T item)
    {
        var feed = _feeds.GetOrAdd(id, new ConcurrentQueue<T>());
        feed.Enqueue(item);
        _echoFeedRepository.AddItem(item);
    }

    public void AddItems(Guid id, IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            AddItem(id, item);
        }
    }

    public void BroadcastItem(T item)
    {
        var feeds = _feeds.ToList();
        foreach (var feed in feeds)
        {
            feed.Value.Enqueue(item);
        }
        _echoFeedRepository.AddItem(item);
    }

    public T GetNext(Guid id)
    {
        var feed = _feeds.GetOrAdd(id, new ConcurrentQueue<T>());
        feed.TryDequeue(out var next);

        return next;
    }

    public void Delete(Guid id)
    {
        _feeds.TryRemove(id, out var feed);
    }
}
