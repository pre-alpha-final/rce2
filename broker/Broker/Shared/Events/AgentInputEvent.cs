using Newtonsoft.Json.Linq;

namespace Broker.Shared.Events;

public class AgentInputEvent : BrokerEventBase
{
    public AgentInputEvent() : base(nameof(AgentInputEvent))
    {
    }

    public Guid AgentId { get; set; }
    public string Contact { get; set; }
    public JToken Payload { get; set; }
}
