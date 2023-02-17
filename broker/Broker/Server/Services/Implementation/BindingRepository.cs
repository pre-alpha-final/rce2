using Broker.Shared.Model;

namespace Broker.Server.Services.Implementation;

public class BindingRepository : IBindingRepository
{
    public HashSet<Binding> Bindings { get; set; } = new();

    public bool AddBinding(Binding binding)
    {
        return Bindings.Add(binding);
    }

    public bool DeleteBinding(Binding binding)
    {
        return Bindings.Remove(binding);
    }
}
