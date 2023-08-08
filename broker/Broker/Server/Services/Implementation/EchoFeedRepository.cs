using Broker.Shared.Events;

namespace Broker.Server.Services.Implementation;

public class EchoFeedRepository<T> : IEchoFeedRepository<T>
{
    private const int MaxSize = 100;
    private static object _lock = new object();
    private Queue<T> _queue = new Queue<T>();

    public void AddItem(T item)
    {
        lock (_lock)
        {
            if (((item is AgentInputEvent) == false) &&
                ((item is AgentSimulatedInputEvent) == false) &&
                ((item is AgentOutputEvent) == false) &&
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

    public List<T> GetAll()
    {
        lock (_lock)
        {
            return _queue.ToList();
        }
    }
}
