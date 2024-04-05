namespace Rce2;

public class Rce2Agent
{
    public Guid Id { get; set; }
    public string? Channel { get; set; }
    public string Name { get; set; } = string.Empty;
    public Dictionary<string, string> Ins { get; set; } = new();
    public Dictionary<string, string> Outs { get; set; } = new();
    public string LastOut { get; set; } = string.Empty;
}
