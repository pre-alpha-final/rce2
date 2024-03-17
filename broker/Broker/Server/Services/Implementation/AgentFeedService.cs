using Broker.Server.Infrastructure;
using Broker.Shared.Model;

namespace Broker.Server.Services.Implementation;

public class AgentFeedService : IAgentFeedService, IDisposable
{
    private Guid? _feedId;
    private const int BatchCount = 10;
    private const int LongPollingTimeout = 30;
    private readonly IAgentFeedRepository _agentFeedRepository;
    private readonly SemaphoreSlim _getSync = new(0);

    public AgentFeedService(IAgentFeedRepository agentFeedRepository)
    {
        _agentFeedRepository = agentFeedRepository;

        PubSub.Hub.Default.Subscribe<AgentFeedUpdate>(this, OnFeedUpdate);
    }

    public void OnFeedUpdate(AgentFeedUpdate agentFeedUpdate)
    {
        if (agentFeedUpdate.AllFeeds || agentFeedUpdate.FeedId == _feedId)
        {
            _getSync.Release();
        }
    }

    public async Task AddItem(Guid feedId, Rce2Message item)
    {
        await PubSub.Hub.Default.PublishAsync(new Activity { Id = feedId });

        _agentFeedRepository.AddItem(feedId, item);
        await PubSub.Hub.Default.PublishAsync(new AgentFeedUpdate
        {
            FeedId = feedId
        });
    }

    public async Task BroadcastItem(Rce2Message item)
    {
        _agentFeedRepository.BroadcastItem(item);
        await PubSub.Hub.Default.PublishAsync(new AgentFeedUpdate());
    }

    public async Task<List<Rce2Message>> GetNext(Guid feedId)
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
        return _agentFeedRepository.Exists(feedId);
    }

    public void Dispose()
    {
        PubSub.Hub.Default.Unsubscribe(this);
    }

    private List<Rce2Message> GetFeed(Guid feedId)
    {
        var feed = new List<Rce2Message>();
        for (var i = 0; i < BatchCount; i++)
        {
            var next = _agentFeedRepository.GetNext(feedId);
            if (next == null)
            {
                break;
            }
            feed.Add(next);
        }

        return feed;
    }
}
