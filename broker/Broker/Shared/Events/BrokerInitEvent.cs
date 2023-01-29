using Broker.Shared.Model;

namespace Broker.Shared.Events;

public class BrokerInitEvent : BrokerEventBase
{
    public BrokerInitEvent() : base(nameof(BrokerInitEvent))
    {
    }

    public List<Agent> Agents { get; set; } = new();
    public List<Binding> Bindings { get; set; } = new();
}
