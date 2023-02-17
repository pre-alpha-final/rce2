using Broker.Shared.Model;

namespace Broker.Shared.Events;

public class BindingDeletedEvent : BrokerEventBase
{
    public BindingDeletedEvent() : base(nameof(BindingDeletedEvent))
    {
    }

    public Binding Binding { get; set; }
}
