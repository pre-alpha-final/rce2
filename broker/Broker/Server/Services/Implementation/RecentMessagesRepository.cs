using Broker.Shared.Events;

namespace Broker.Server.Services.Implementation;

public class RecentMessagesRepository : IRecentMessagesRepository
{
    private const int MaxSize = 10;
    private static object _lock = new object();
    private Queue<BrokerEventBase> _queue = new Queue<BrokerEventBase>();

    public void AddItem(BrokerEventBase item)
    {
        lock (_lock)
        {
            if (((item is AgentInputReceivedEvent) == false) &&
                ((item is AgentSimulatedInputEvent) == false) &&
                ((item is AgentOutputReceivedEvent) == false) &&
                ((item is AgentSimulatedOutputEvent) == false))
            {
                return;
            }

            if (_queue.Count >= MaxSize)
            {
                _queue.Dequeue();
            }
            _queue.Enqueue(item);
        }
    }

    public List<BrokerEventBase> GetAll()
    {
        lock (_lock)
        {
            return _queue.ToList();
        }
    }
}
