namespace Broker.Server.Services;

public interface IFeedRepository<T> where T : class
{
    bool Exists(Guid id);
    void AddItem(Guid id, T item);
    void AddItems(Guid id, IEnumerable<T> items);
    void BroadcastItem(T item);
    T GetNext(Guid id);
    void Delete(Guid id);
}
