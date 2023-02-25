namespace Broker.Server.Services;

public interface IFeedService<T> where T : class
{
    Task AddItem(Guid id, T item);
    Task AddItems(Guid id, IEnumerable<T> items);
    Task BroadcastItem(T item);
    Task<List<T>> GetNext(Guid id);
    bool Exists(Guid id);
}
