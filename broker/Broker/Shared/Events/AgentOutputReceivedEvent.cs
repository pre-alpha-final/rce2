using Newtonsoft.Json.Linq;

namespace Broker.Shared.Events;

public class AgentOutputReceivedEvent : BrokerEventBase
{
    private const int PayloadMaxLength = 500;

    public AgentOutputReceivedEvent(JToken payload) : base(nameof(AgentOutputReceivedEvent))
    {
        var payloadString = payload.ToString();
        Payload = payloadString.Length > PayloadMaxLength
            ? (payloadString.Substring(0, PayloadMaxLength) + " (...)")
            : payloadString;
    }

    public Guid AgentId { get; set; }
    public string Contact { get; set; }
    public string Payload { get; set; }
}
