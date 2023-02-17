using Broker.Server.Infrastructure;
using Broker.Shared.Events;

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
        });
    }

    public async Task BroadcastItem(T item)
    {
        _feedRepository.BroadcastItem(item);
        await PubSub.Hub.Default.PublishAsync(new FeedUpdate());
    }

    public async Task<List<T>> GetNext(Guid id)
    {
        _requestId = id;

        if (_feedRepository.Exists(id) == false)
        {
            _feedRepository.AddItem(id, new BrokerInitEvent
            {
                Agents = new()
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Name = "Agent1",
                        Ins = new()
                        {
                            { "jakies wejscie", "void" }
                        },
                        Outs = new()
                        {
                            { "cos wyjscie", "number" }
                        },
                        LastOut = "foo1",
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Name = "Agent1",
                        Ins = new()
                        {
                            { "jakies wejscie", "void" }
                        },
                        Outs = new()
                        {
                            { "cos wyjscie", "number" }
                        },
                        LastOut = "foo1",
                    },                    new()
                    {
                        Id = Guid.NewGuid(),
                        Name = "Agent1",
                        Ins = new()
                        {
                            { "jakies wejscie", "void" }
                        },
                        Outs = new()
                        {
                            { "cos wyjscie", "number" }
                        },
                        LastOut = "foo1",
                    },                    new()
                    {
                        Id = Guid.NewGuid(),
                        Name = "Agent1",
                        Ins = new()
                        {
                            { "jakies wejscie", "void" }
                        },
                        Outs = new()
                        {
                            { "cos wyjscie", "number" }
                        },
                        LastOut = "foo1",
                    },                    new()
                    {
                        Id = Guid.NewGuid(),
                        Name = "Agent1",
                        Ins = new()
                        {
                            { "jakies wejscie", "void" }
                        },
                        Outs = new()
                        {
                            { "cos wyjscie", "number" }
                        },
                        LastOut = "foo1",
                    },                    new()
                    {
                        Id = Guid.NewGuid(),
                        Name = "Agent1",
                        Ins = new()
                        {
                            { "jakies wejscie", "void" }
                        },
                        Outs = new()
                        {
                            { "cos wyjscie", "number" }
                        },
                        LastOut = "foo1",
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Name = "Agent2",
                        Ins = new()
                        {
                            { "in21", "string" }
                        },
                        Outs = new()
                        {
                            { "out21", "string-list" }
                        },
                        LastOut = "foo2",
                    },
                },
                Bindings = new()
                {
                    new()
                    {
                        OutId = Guid.NewGuid(),
                        OutName = "Agent1",
                        OutContact = "out11",
                        InId = Guid.NewGuid(),
                        InName = "Agent1",
                        InContact = "in11",
                    },
                    new()
                    {
                        OutId = Guid.NewGuid(),
                        OutName = "Agent2",
                        OutContact = "out21",
                        InId = Guid.NewGuid(),
                        InName = "Agent1",
                        InContact = "in11",
                    },
                                        new()
                    {
                        OutId = Guid.NewGuid(),
                        OutName = "Agent2",
                        OutContact = "out21",
                        InId = Guid.NewGuid(),
                        InName = "Agent1",
                        InContact = "in11",
                    },
                                                            new()
                    {
                        OutId = Guid.NewGuid(),
                        OutName = "Agent2",
                        OutContact = "out21",
                        InId = Guid.NewGuid(),
                        InName = "Agent1",
                        InContact = "in11",
                    },
                                                                                new()
                    {
                        OutId = Guid.NewGuid(),
                        OutName = "Agent2",
                        OutContact = "out21",
                        InId = Guid.NewGuid(),
                        InName = "Agent1",
                        InContact = "in11",
                    },
                }
            } as T);
        }

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
        if (feedUpdate.Id == null || feedUpdate.Id == _requestId)
        {
            _getSync.Release();
        }
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
