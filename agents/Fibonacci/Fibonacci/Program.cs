using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace Fibonacci;

internal class Program
{
    private static Guid AgentId = Guid.NewGuid();
    private static string Address = $"https://localhost:7113/api/agent/{AgentId}";

    private static int _previousNumber;

    private static void Main(string[] args)
    {
        _ = Task.Run(FeedHandler);
        Console.ReadLine();
    }

    private static async Task FeedHandler()
    {
        using var httpClient = new HttpClient();
        while (true)
        {
            try
            {
                var feed = await httpClient.GetAsync(Address);
                var content = await feed.Content.ReadAsStringAsync();
                var rce2Messages = JsonConvert.DeserializeObject<List<Rce2Message>>(content);
                foreach (var rce2Message in rce2Messages)
                {
                    switch (rce2Message.Contact)
                    {
                        case "input_number":
                            await TryRun(() => HandleFibanacciMessage(rce2Message.Payload));
                            break;

                        default:
                            if (rce2Message.Type == Rce2Types.WhoIs)
                            {
                                await TryRun(HandleWhoIsMessage);
                            }
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                await Task.Delay(1000);
                // ignore
            }
        }
    }

    private static async Task HandleWhoIsMessage()
    {
        using var httpClient = new HttpClient();
        await httpClient.PostAsync(Address, new StringContent(JsonConvert.SerializeObject(new Rce2Message
        {
            Type = Rce2Types.WhoIs,
            Payload = JObject.FromObject(new Agent
            {
                Id = AgentId,
                Name = "Fibanacci",
                Ins = new()
                {
                    { "input_number", Rce2Types.Number }
                },
                Outs = new()
                {
                    { "output_number", Rce2Types.Number }
                }
            }),
        }), Encoding.UTF8, "application/json"));
    }

    private static async Task HandleFibanacciMessage(JToken payload)
    {
        await Task.Delay(1000);

        var nextNumber = _previousNumber + payload["data"].ToObject<int>();
        _previousNumber = payload["data"].ToObject<int>();

        using var httpClient = new HttpClient();
        await httpClient.PostAsync(Address, new StringContent(JsonConvert.SerializeObject(new Rce2Message
        {
            Type = Rce2Types.Number,
            Contact = "output_number",
            Payload = JObject.FromObject(new { data = nextNumber })
        }), Encoding.UTF8, "application/json"));
    }

    private static async Task TryRun(Func<Task> taskFunc)
    {
        try
        {
            await taskFunc();
        }
        catch
        {
            // Ignore
        }
    }
}
