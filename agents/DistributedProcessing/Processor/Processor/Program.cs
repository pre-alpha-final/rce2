using Fibonacci;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace Processor;

internal class Program
{
    private static Guid AgentId = Guid.NewGuid();
    private static string Address = $"https://localhost:7113/api/agent/{AgentId}";
    private static Guid RequestId;

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
                        case "start":
                            await TryRun(HandleStart);
                            break;

                        case "data_received":
                            await TryRun(() => HandleDataReceived(rce2Message.Payload));
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
                Name = "Processor",
                Ins = new()
                {
                    { "start", Rce2Types.Void },
                    { "data_received", Rce2Types.StringList }
                },
                Outs = new()
                {
                    { "request_data", Rce2Types.String },
                    { "data_processed", Rce2Types.String }
                }
            }),
        }), Encoding.UTF8, "application/json"));
    }

    private static async Task HandleStart()
    {
        await RequestNextChunk();
    }

    private static async Task HandleDataReceived(JToken payload)
    {
        var payloadList = payload["data"].ToObject<List<string>>();
        if (payloadList[0] != $"{RequestId}")
        {
            return;
        }

        await Task.Delay(1000); // Processing
        await SignalProcessed(payloadList[1]);

        await RequestNextChunk();
    }

    private static async Task RequestNextChunk()
    {
        RequestId = Guid.NewGuid();

        using var httpClient = new HttpClient();
        await httpClient.PostAsync(Address, new StringContent(JsonConvert.SerializeObject(new Rce2Message
        {
            Type = Rce2Types.StringList,
            Contact = "request_data",
            Payload = JObject.FromObject(new { data = $"{RequestId}" })
        }), Encoding.UTF8, "application/json"));
    }

    private static async Task SignalProcessed(string chunk)
    {
        using var httpClient = new HttpClient();
        await httpClient.PostAsync(Address, new StringContent(JsonConvert.SerializeObject(new Rce2Message
        {
            Type = Rce2Types.StringList,
            Contact = "data_processed",
            Payload = JObject.FromObject(new { data = $"Processed {chunk}" })
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
