using Newtonsoft.Json.Linq;

namespace Broker.Shared.Events;

public class AgentSimulatedInputEvent : BrokerEventBase
{
    public AgentSimulatedInputEvent() : base(nameof(AgentSimulatedInputEvent))
    {
    }

    public Guid AgentId { get; set; }
    public string Contact { get; set; }
    public JToken Payload { get; set; }
}
