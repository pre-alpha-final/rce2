using Newtonsoft.Json.Linq;

namespace Broker.Shared.Events;

public class AgentInputReceivedEvent : BrokerEventBase
{
    private const int PayloadMaxLength = 500;

    public AgentInputReceivedEvent(JToken payload) : base(nameof(AgentInputReceivedEvent))
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
