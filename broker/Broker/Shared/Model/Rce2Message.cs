using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Broker.Shared.Model;

public class Rce2Message
{
    public string Type { get; set; }
    public string? Contact { get; set; }
    public JToken Payload { get; set; }

    public Rce2Message Clone()
    {
        return JsonConvert.DeserializeObject<Rce2Message>(JsonConvert.SerializeObject(this));
    }
}
