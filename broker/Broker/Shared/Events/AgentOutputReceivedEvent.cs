using Newtonsoft.Json.Linq;

namespace Broker.Shared.Events;

public class AgentOutputReceivedEvent : BrokerEventBase
{
    public AgentOutputReceivedEvent() : base(nameof(AgentOutputReceivedEvent))
    {
    }

    public Guid AgentId { get; set; }
    public string Contact { get; set; }
    public JToken Payload { get; set; }
}
