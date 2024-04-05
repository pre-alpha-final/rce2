using Broker.Server.Infrastructure;
using Broker.Shared.Events;

namespace Broker.Server.Services.Implementation;

public class JanitorService : IJanitorService
{
    private readonly IAgentFeedRepository _agentFeedRepository;
    private readonly IBrokerFeedRepository _brokerFeedRepository;
    private readonly IBindingRepository _bindingRepository;
    private readonly IActiveAgentCache _activeAgentCache;

    // TODO Activity is just an Id now, consider adding a type so that it is immediately clear what to clean up
    public JanitorService(IAgentFeedRepository agentFeedRepository, IBrokerFeedRepository brokerFeedRepository,
        IBindingRepository bindingRepository, IActiveAgentCache activeAgentCache)
    {
        _agentFeedRepository = agentFeedRepository;
        _brokerFeedRepository = brokerFeedRepository;
        _bindingRepository = bindingRepository;
        _activeAgentCache = activeAgentCache;

        PubSub.Hub.Default.Subscribe<Activity>(this, OnActivity);
    }

    public Dictionary<Guid, DateTimeOffset> ActivityDictionary { get; set; } = new();

    public async Task Run()
    {
        while (true)
        {
            try
            {
                await Task.Delay(TimeSpan.FromMinutes(1));
                var inactiveEntities = ActivityDictionary
                    .Where(e => e.Value < DateTimeOffset.UtcNow - TimeSpan.FromHours(1))
                    .ToList();
                foreach (var inactiveEntity in inactiveEntities)
                {
                    await HandleBindings(inactiveEntity);
                    await HandleAgents(inactiveEntity);
                    HandleActiveAgentsCache(inactiveEntity);
                    HandleBrokers(inactiveEntity);
                    HandleActivityDictionary(inactiveEntity);
                }
            }
            catch (Exception e)
            {
                // ignore
            }
        }
    }

    private async Task HandleBindings(KeyValuePair<Guid, DateTimeOffset> inactiveEntity)
    {
        var bindings = _bindingRepository.GetAll();
        foreach (var binding in bindings)
        {
            if (binding.InId == inactiveEntity.Key || binding.OutId == inactiveEntity.Key)
            {
                binding.IsActive = false;
                _bindingRepository.UpdateBinding(binding);
                await BrokerBroadcast(new BindingUpdatedEvent
                {
                    Binding = binding,
                });
            }
        }
    }

    private async Task HandleAgents(KeyValuePair<Guid, DateTimeOffset> inactiveEntity)
    {
        if (_agentFeedRepository.Delete(inactiveEntity.Key))
        {
            await BrokerBroadcast(new AgentDeletedEvent
            {
                Id = inactiveEntity.Key,
            });
        }
    }

    private void HandleActiveAgentsCache(KeyValuePair<Guid, DateTimeOffset> inactiveEntity)
    {
        _activeAgentCache.Remove(inactiveEntity.Key);
    }

    private void HandleBrokers(KeyValuePair<Guid, DateTimeOffset> inactiveEntity)
    {
        _brokerFeedRepository.Delete(inactiveEntity.Key);
    }

    private void HandleActivityDictionary(KeyValuePair<Guid, DateTimeOffset> inactiveEntity)
    {
        ActivityDictionary.Remove(inactiveEntity.Key);
    }

    private async Task OnActivity(Activity activity)
    {
        ActivityDictionary[activity.Id] = activity.Timestamp;
    }

    private async Task BrokerBroadcast(BrokerEventBase item)
    {
        _brokerFeedRepository.BroadcastItem(item);
        await PubSub.Hub.Default.PublishAsync(new BrokerFeedUpdate());
    }
}
