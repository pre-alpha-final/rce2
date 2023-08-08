using Newtonsoft.Json.Linq;

namespace Broker.Shared.Events;

public class AgentSimulatedOutputEvent : BrokerEventBase
{
    public AgentSimulatedOutputEvent() : base(nameof(AgentSimulatedOutputEvent))
    {
    }

    public Guid AgentId { get; set; }
    public string Contact { get; set; }
    public JToken Payload { get; set; }
}
