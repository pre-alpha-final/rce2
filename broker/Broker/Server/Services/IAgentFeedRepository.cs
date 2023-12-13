using Broker.Shared.Model;

namespace Broker.Server.Services;

public interface IAgentFeedRepository
{
    bool Exists(Guid feedId);
    void AddItem(Guid feedId, Rce2Message item);
    void AddItems(Guid feedId, IEnumerable<Rce2Message> items);
    void BroadcastItem(Rce2Message item);
    Rce2Message GetNext(Guid feedId);
    bool Delete(Guid feedId);
}
