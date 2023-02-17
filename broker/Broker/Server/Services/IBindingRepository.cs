using Broker.Shared.Model;

namespace Broker.Server.Services;

public interface IBindingRepository
{
    bool AddBinding(Binding binding);
    bool DeleteBinding(Binding binding);
}
