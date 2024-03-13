using Newtonsoft.Json.Linq;

namespace Rce2;

public class Rce2Message
{
    public string Type { get; set; } = string.Empty;
    public string? Contact { get; set; }
    public JToken? Payload { get; set; }
}
