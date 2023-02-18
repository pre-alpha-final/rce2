using Broker.Server.Infrastructure;
using Broker.Shared.Events;
using Broker.Shared.Model;

namespace Broker.Server.Services.Implementation;

public class FeedService<T> : IFeedService<T>, IDisposable where T : class
{
    private const int BatchCount = 10;
    private const int LongPollingTimeout = 30;
    private readonly IFeedRepository<T> _feedRepository;
    private readonly SemaphoreSlim _getSync = new(0);
    private Guid? _requestId;

    public FeedService(IFeedRepository<T> feedRepository)
    {
        _feedRepository = feedRepository;
        PubSub.Hub.Default.Subscribe<FeedUpdate>(this, OnFeedUpdate);
    }

    public async Task AddItem(Guid id, T item)
    {
        _feedRepository.AddItem(id, item);
        await PubSub.Hub.Default.PublishAsync(new FeedUpdate
        {
            Id = id,
            Type = typeof(T),
        });
    }

    public async Task BroadcastItem(T item)
    {
        _feedRepository.BroadcastItem(item);
        await PubSub.Hub.Default.PublishAsync(new FeedUpdate
        {
            Type = typeof(T),
        });
    }

    public async Task<List<T>> GetNext(Guid id)
    {
        _requestId = id;

        var feed = GetFeed(id);
        if (feed.Any() == false)
        {
            var delay = Task.Delay(TimeSpan.FromSeconds(LongPollingTimeout));
            await Task.WhenAny(delay, _getSync.WaitAsync());
            if (delay.IsCompleted == false)
            {
                feed = GetFeed(id);
            }
        }

        return feed;
    }

    public void OnFeedUpdate(FeedUpdate feedUpdate)
    {
        if (feedUpdate.Type == typeof(T) && (feedUpdate.Id == null || feedUpdate.Id == _requestId))
        {
            _getSync.Release();
        }
    }

    public bool Exists(Guid id)
    {
        return _feedRepository.Exists(id);
    }

    public void Dispose()
    {
        PubSub.Hub.Default.Unsubscribe(this);
    }

    private List<T> GetFeed(Guid id)
    {
        var feed = new List<T>();
        for (var i = 0; i < BatchCount; i++)
        {
            var next = _feedRepository.GetNext(id);
            if (next == null)
            {
                break;
            }
            feed.Add(next);
        }

        return feed;
    }
}
