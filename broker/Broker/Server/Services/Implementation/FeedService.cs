using Broker.Shared.Infrastructure;

namespace Broker.Server.Services.Implementation;

public class FeedService : IFeedService
{
    private const int BatchCount = 10;
    private const int LongPollingTimeout = 30;
    private readonly IFeedRepository _feedRepository;
    private readonly SemaphoreSlim _getSync = new SemaphoreSlim(0, 1);

    public FeedService(IFeedRepository feedRepository)
    {
        _feedRepository = feedRepository;
    }

    public async Task AddItem(Guid id, Rce2Message rce2Message)
    {
        _feedRepository.AddItem(id, rce2Message);
    }

    public async Task<List<Rce2Message>> GetNext(Guid id)
    {
        if (_feedRepository.Exists(id) == false)
        {
            // feed initial state
            _feedRepository.AddItem(id, new Rce2Message { Type = Rce2Types.Agent, Payload = new List<object> { new Agent() } });
        }

        var feed = GetFeed(id);
        if (feed.Any() == false)
        {
            await Task.WhenAny(_getSync.WaitAsync(), Task.Delay(TimeSpan.FromSeconds(LongPollingTimeout)));
            feed = GetFeed(id);
        }

        return feed;
    }

    private List<Rce2Message> GetFeed(Guid id)
    {
        var feed = new List<Rce2Message>();
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

    private void TryRelease()
    {
        try
        {
            _getSync.Release();
        }
        catch (Exception e)
        {
            // ignore
        }
    }
}
