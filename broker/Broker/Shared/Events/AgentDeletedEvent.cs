namespace Broker.Shared.Events;

public class AgentDeletedEvent : BrokerEventBase
{
    public AgentDeletedEvent() : base(nameof(AgentDeletedEvent))
    {
    }

    public Guid Id { get; set; }
}
