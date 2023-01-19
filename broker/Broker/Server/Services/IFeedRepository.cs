using Broker.Shared.Infrastructure;

namespace Broker.Server.Services;

public interface IFeedRepository
{
    bool Exists(Guid id);
    void AddItem(Guid id, Rce2Message rce2Message);
    Rce2Message GetNext(Guid id);
}
