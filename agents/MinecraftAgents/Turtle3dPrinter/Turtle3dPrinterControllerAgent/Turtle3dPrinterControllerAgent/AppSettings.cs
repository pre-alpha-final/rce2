namespace Turtle3dPrinterControllerAgent;

public class AppSettings
{
    public AppSettings()
    {
        AgentId = Guid.Parse("11111111-2222-3333-4444-100000000001");
        Address = $"http://localhost:5113/api/agent/{AgentId}";
        PrintFolder = "c:/d/print/";
    }

    public Guid AgentId { get; set; }
    public string Address { get; set; }
    public string PrintFolder { get; set; }
}
