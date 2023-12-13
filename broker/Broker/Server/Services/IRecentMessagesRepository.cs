using Broker.Shared.Events;

namespace Broker.Server.Services;

public interface IRecentMessagesRepository
{
    void AddItem(BrokerEventBase item);
    List<BrokerEventBase> GetAll();
}
