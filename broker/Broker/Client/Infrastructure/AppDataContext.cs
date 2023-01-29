using Broker.Shared.Model;

namespace Broker.Client.Infrastructure;

public class AppDataContext
{
    public string Code { get; set; }
    public bool AuthorizationFailure { get; set; }
    public List<Agent> Agents { get; set; }
    public List<Binding> Bindings { get; set; }
}
