namespace Rce2;

public class RenderMessageReceived
{
    public string Rce2RequestId { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public byte[]? Body { get; set; } = [];
}
