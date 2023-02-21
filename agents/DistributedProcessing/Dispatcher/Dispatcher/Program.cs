using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace Dispatcher;

internal class Program
{
    private static Guid AgentId = Guid.NewGuid();
    private static string Address = $"https://localhost:7113/api/agent/{AgentId}";

    private static List<string> DataChunks = new() { "DataChunk1", "DataChunk2", "DataChunk3", "DataChunk4", "DataChunk5", "DataChunk6" };

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
                        case "chunk_request":
                            await TryRun(() => HandleChunkRequest(rce2Message.Payload));
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
                Name = "Dispatcher",
                Ins = new()
                {
                    { "chunk_request", Rce2Types.String }
                },
                Outs = new()
                {
                    { "data_chunk", Rce2Types.StringList }
                }
            }),
        }), Encoding.UTF8, "application/json"));
    }

    private static async Task HandleChunkRequest(JToken payload)
    {
        var chunkRequestId = payload["data"].ToObject<Guid>();
        var chunk = DataChunks.First(); // obviously not locked and will explode on empty - just an example
        DataChunks.RemoveAt(0);

        using var httpClient = new HttpClient();
        await httpClient.PostAsync(Address, new StringContent(JsonConvert.SerializeObject(new Rce2Message
        {
            Type = Rce2Types.StringList,
            Contact = "data_chunk",
            Payload = JObject.FromObject(new { data = new[] { $"{chunkRequestId}", chunk } })
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
