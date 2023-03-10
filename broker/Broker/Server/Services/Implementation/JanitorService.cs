using Broker.Server.Infrastructure;
using Broker.Shared.Events;
using Broker.Shared.Model;

namespace Broker.Server.Services.Implementation;

public class JanitorService : IJanitorService
{
    private readonly IFeedRepository<Rce2Message> _agentFeedRepository;
    private readonly IFeedRepository<BrokerEventBase> _brokerFeedRepository;
    private readonly IBindingRepository _bindingRepository;

    public JanitorService(IFeedRepository<Rce2Message> agentFeedRepository, IFeedRepository<BrokerEventBase> brokerFeedRepository,
        IBindingRepository bindingRepository)
    {
        _agentFeedRepository = agentFeedRepository;
        _brokerFeedRepository = brokerFeedRepository;
        _bindingRepository = bindingRepository;

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
                    .Where(e => e.Value < DateTimeOffset.UtcNow - TimeSpan.FromMinutes(1))
                    .ToList();
                foreach (var inactiveEntity in inactiveEntities)
                {
                    await HandleBindings(inactiveEntity);
                    await HandleAgents(inactiveEntity);
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
                _bindingRepository.DeleteBinding(binding);
                await BrokerBroadcast(new BindingDeletedEvent
                {
                    Binding = binding,
                });
            }
        }
    }

    private async Task HandleAgents(KeyValuePair<Guid, DateTimeOffset> inactiveEntity)
    {
        _agentFeedRepository.Delete(inactiveEntity.Key);
        await BrokerBroadcast(new AgentDeletedEvent
        {
            Id = inactiveEntity.Key,
        });
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
        await PubSub.Hub.Default.PublishAsync(new FeedUpdate
        {
            Type = typeof(BrokerEventBase),
        });
    }
}
