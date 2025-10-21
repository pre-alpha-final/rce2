namespace Rce2;

public class Rce2Agent
{
    public Guid Id { get; set; }
    public List<string> Channels { get; set; } = new();
    public string Name { get; set; }
    public Dictionary<string, string> Ins { get; set; } = new();
    public Dictionary<string, string> Outs { get; set; } = new();
    public string LastOut { get; set; }
}
