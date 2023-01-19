using Broker.Shared.Infrastructure;

namespace Broker.Server.Services;

public interface IFeedService
{
    Task AddItem(Guid id, Rce2Message rce2Message);
    Task<List<Rce2Message>> GetNext(Guid id);
}
