using System.Collections.Generic;

namespace Broker.Server.Services;

public interface IEchoFeedRepository<T>
{
    void AddItem(T item);
    List<T> GetAll();
}
