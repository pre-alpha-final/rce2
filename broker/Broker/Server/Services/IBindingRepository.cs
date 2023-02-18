using Broker.Shared.Model;

namespace Broker.Server.Services;

public interface IBindingRepository
{
    List<Binding> GetAll();
    List<Binding> GetBindingsFrom(Guid id, string contact);
    bool AddBinding(Binding binding);
    bool DeleteBinding(Binding binding);
}
