using Broker.Shared.Events;

namespace Broker.Server.Services;

public interface IBrokerFeedRepository
{
    bool Exists(Guid feedId);
    void AddItem(Guid feedId, BrokerEventBase item);
    void AddItems(Guid feedId, IEnumerable<BrokerEventBase> items);
    void BroadcastItem(BrokerEventBase item);
    BrokerEventBase GetNext(Guid feedId);
    bool Delete(Guid feedId);
}
