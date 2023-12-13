using Broker.Shared.Model;

namespace Broker.Server.Services;

public interface IAgentFeedService
{
    Task AddItem(Guid feedId, Rce2Message item);
    Task BroadcastItem(Rce2Message item);
    Task<List<Rce2Message>> GetNext(Guid feedId);
    bool Exists(Guid feedId);
}
