using Newtonsoft.Json.Linq;

namespace PopupAgent.Rce2
{
    public class Rce2Message
    {
        public string Type { get; set; }
        public string Contact { get; set; }
        public JToken Payload { get; set; }
    }
}