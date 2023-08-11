using Broker.Shared.Model;

namespace Broker.Server.Services.Implementation;

public class BindingRepository : IBindingRepository
{
    public HashSet<Binding> Bindings { get; set; } = new();

    public List<Binding> GetAll()
    {
        return Bindings.ToList();
    }

    public List<Binding> GetBindingsFrom(Guid id, string contact)
    {
        return Bindings
            .Where(e => e.OutId == id)
            .Where(e => e.OutContact == contact)
            .ToList();
    }

    public bool AddBinding(Binding binding)
    {
        return Bindings.Add(binding);
    }

    public bool UpdateBinding(Binding binding)
    {
        Bindings.Remove(binding);
        return Bindings.Add(binding);
    }

    public bool DeleteBinding(Binding binding)
    {
        return Bindings.Remove(binding);
    }
}
