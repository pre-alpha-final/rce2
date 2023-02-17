using Broker.Shared.Model;

namespace Broker.Shared.Events;

public class AgentUpdatedEvent : BrokerEventBase
{
    public AgentUpdatedEvent() : base(nameof(AgentUpdatedEvent))
    {
    }

    public Agent Agent { get; set; }
}
