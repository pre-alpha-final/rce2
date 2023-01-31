namespace Broker.Shared.Model;

public class Rce2Message
{
    // Implicit direction
    public string Type { get; set; }
    public string Contact { get; set; }
    public List<object> Payload { get; set; }
}
