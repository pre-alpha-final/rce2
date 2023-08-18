namespace Broker.Shared.Events;

public class BrokerEventBase
{
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    public string BrokerEventType { get; set; }

    public BrokerEventBase(string brokerEventType)
    {
        BrokerEventType = brokerEventType;
    }
}
