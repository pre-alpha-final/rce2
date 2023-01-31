using System.Collections.Concurrent;

namespace Broker.Server.Services.Implementation;

public class FeedRepository<T> : IFeedRepository<T> where T : class
{
    private readonly ConcurrentDictionary<Guid, ConcurrentQueue<T>> _feeds = new();

    public bool Exists(Guid id)
    {
        return _feeds.ContainsKey(id);
    }

    public void AddItem(Guid id, T item)
    {
        var feed = _feeds.GetOrAdd(id, new ConcurrentQueue<T>());
        feed.Enqueue(item);
    }

    public T GetNext(Guid id)
    {
        var feed = _feeds.GetOrAdd(id, new ConcurrentQueue<T>());
        feed.TryDequeue(out var next);

        return next;
    }
}
