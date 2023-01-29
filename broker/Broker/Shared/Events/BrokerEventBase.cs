namespace Broker.Shared.Events;

public class BrokerEventBase
{
	public string BrokerEventType { get; set; }

	public BrokerEventBase(string brokerEventType)
	{
		BrokerEventType = brokerEventType;
	}
}
