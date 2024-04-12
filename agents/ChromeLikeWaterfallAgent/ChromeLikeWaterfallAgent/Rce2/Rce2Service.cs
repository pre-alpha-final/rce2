using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text;
using PubSub;
using ChromeLikeWaterfallAgent;

namespace Rce2;

public class Rce2Service
{
    private Guid AgentId { get; } = Guid.NewGuid();
    private string Address => $"https://localhost:7113/api/agent/{AgentId}";

    private readonly IHttpClientFactory _httpClientFactory;

    public Rce2Service(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public void Run()
    {
        Task.Run(FeedHandler);
    }

    private async Task FeedHandler()
    {
        while (true)
        {
            try
            {
                var feed = await _httpClientFactory.CreateClient().GetAsync(Address);
                var content = await feed.Content.ReadAsStringAsync();
                var rce2Messages = JsonConvert.DeserializeObject<List<Rce2Message>>(content)!;
                foreach (var rce2Message in rce2Messages)
                {
                    switch (rce2Message.Contact)
                    {
                        case Rce2Contacts.Ins.NameStartStop:
                            await TryRun(() => HandleNameStartStop(rce2Message.Payload));
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

    private async Task HandleNameStartStop(JToken? payload)
    {
        var payloadData = payload?["data"]?.ToObject<List<string>>();
        if (payloadData == null)
        {
            return;
        }

        await Hub.Default.PublishAsync(new NameStartStop
        {
            Name = payloadData.GetValue("name"),
            Start = long.TryParse(payloadData.GetValue("start"), out var start) ? start : 0,
            Stop = long.TryParse(payloadData.GetValue("stop"), out var stop) ? stop : 0,
        });
    }

    private async Task HandleWhoIsMessage()
    {
        await _httpClientFactory.CreateClient().PostAsync(Address, new StringContent(JsonConvert.SerializeObject(new Rce2Message
        {
            Type = Rce2Types.WhoIs,
            Payload = JObject.FromObject(new Rce2Agent
            {
                Id = AgentId,
                Channels = new() { "waterfall" },
                Name = "Chrome-like Waterfall",
                Ins = new()
                {
                    { Rce2Contacts.Ins.NameStartStop, Rce2Types.StringList },
                },
                Outs = new()
                {
                }
            }),
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
