namespace Broker.Server.Services.Implementation;

public class EchoFeedRepositoryDummy<T> : IEchoFeedRepository<T>
{
    public void AddItem(T item)
    {
        return;
    }

    public List<T> GetAll()
    {
        return new List<T>();
    }
}
