namespace Broker.Shared.Model;

public class Binding
{
    public Guid OutId { get; set; }
    public string OutName { get; set; }
    public string OutOut { get; set; }
    public Guid InId { get; set; }
    public string InName { get; set; }
    public string InIn { get; set; }
}
