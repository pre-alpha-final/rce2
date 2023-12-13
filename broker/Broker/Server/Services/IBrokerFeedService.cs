using Broker.Shared.Events;

namespace Broker.Server.Services;

public interface IBrokerFeedService
{
    Task AddItems(Guid feedId, IEnumerable<BrokerEventBase> items);
    Task BroadcastItem(BrokerEventBase item);
    Task<List<BrokerEventBase>> GetNext(Guid feedId);
    bool Exists(Guid feedId);
}
