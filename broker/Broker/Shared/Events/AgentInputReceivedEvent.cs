using Newtonsoft.Json.Linq;

namespace Broker.Shared.Events;

public class AgentInputReceivedEvent : BrokerEventBase
{
    public AgentInputReceivedEvent() : base(nameof(AgentInputReceivedEvent))
    {
    }

    public Guid AgentId { get; set; }
    public string Contact { get; set; }
    public JToken Payload { get; set; }
}
