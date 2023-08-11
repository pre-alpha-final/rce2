using Broker.Shared.Model;

namespace Broker.Shared.Events;

public class BindingUpdatedEvent : BrokerEventBase
{
    public BindingUpdatedEvent() : base(nameof(BindingUpdatedEvent))
    {
    }

    public Binding Binding { get; set; }
}
