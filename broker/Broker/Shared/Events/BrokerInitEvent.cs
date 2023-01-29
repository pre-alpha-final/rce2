using Broker.Shared.Model;

namespace Broker.Shared.Events;

public class BrokerInitEvent : BrokerEventBase
{
    public List<Agent> Agents { get; set; } = new();
    public List<Binding> Bindings { get; set; } = new();
}
