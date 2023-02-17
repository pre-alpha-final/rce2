using Broker.Shared.Model;

namespace Broker.Server.Services;

public interface IBindingRepository
{
    List<Binding> GetBindingsFrom(Guid id);
    bool AddBinding(Binding binding);
    bool DeleteBinding(Binding binding);
}
