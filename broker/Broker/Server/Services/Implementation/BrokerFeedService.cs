using Broker.Server.Infrastructure;
using Broker.Shared.Events;

namespace Broker.Server.Services.Implementation;

public class BrokerFeedService : IBrokerFeedService, IDisposable
{
    private Guid? _feedId;
    private const int BatchCount = 10;
    private const int LongPollingTimeout = 30;
    private readonly IBrokerFeedRepository _brokerFeedRepository;
    private readonly SemaphoreSlim _getSync = new(0);

    public BrokerFeedService(IBrokerFeedRepository brokerFeedRepository)
    {
        _brokerFeedRepository = brokerFeedRepository;

        PubSub.Hub.Default.Subscribe<BrokerFeedUpdate>(this, OnFeedUpdate);
    }

    public void OnFeedUpdate(BrokerFeedUpdate brokerFeedUpdate)
    {
        if (brokerFeedUpdate.AllFeeds || brokerFeedUpdate.FeedId == _feedId)
        {
            _getSync.Release();
        }
    }

    public async Task AddItems(Guid feedId, IEnumerable<BrokerEventBase> items)
    {
        _brokerFeedRepository.AddItems(feedId, items);
        await PubSub.Hub.Default.PublishAsync(new BrokerFeedUpdate
        {
            FeedId = feedId
        });
    }

    public async Task BroadcastItem(BrokerEventBase item)
    {
        _brokerFeedRepository.BroadcastItem(item);
        await PubSub.Hub.Default.PublishAsync(new BrokerFeedUpdate());
    }

    public async Task<List<BrokerEventBase>> GetNext(Guid feedId)
    {
        _feedId = feedId;
        await PubSub.Hub.Default.PublishAsync(new Activity { Id = feedId });

        var feed = GetFeed(feedId);
        if (feed.Any() == false)
        {
            var delay = Task.Delay(TimeSpan.FromSeconds(LongPollingTimeout));
            await Task.WhenAny(delay, _getSync.WaitAsync());
            if (delay.IsCompleted == false)
            {
                feed = GetFeed(feedId);
            }
        }

        return feed;
    }

    public bool Exists(Guid feedId)
    {
        return _brokerFeedRepository.Exists(feedId);
    }

    public void Dispose()
    {
        PubSub.Hub.Default.Unsubscribe(this);
    }

    private List<BrokerEventBase> GetFeed(Guid feedId)
    {
        var feed = new List<BrokerEventBase>();
        for (var i = 0; i < BatchCount; i++)
        {
            var next = _brokerFeedRepository.GetNext(feedId);
            if (next == null)
            {
                break;
            }
            feed.Add(next);
        }

        return feed;
    }
}
