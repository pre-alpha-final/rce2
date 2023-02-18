using Newtonsoft.Json.Linq;

namespace Broker.Shared.Events;

public class AgentOutputEvent : BrokerEventBase
{
    public AgentOutputEvent() : base(nameof(AgentOutputEvent))
    {
    }

    public Guid AgentId { get; set; }
    public JToken Payload { get; set; }
}
