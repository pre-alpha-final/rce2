using Broker.Shared.Model;

namespace Broker.Shared.Events;

public class BindingAddedEvent : BrokerEventBase
{
    public BindingAddedEvent() : base(nameof(BindingAddedEvent))
    {
    }

    public Binding Binding { get; set; }
}
