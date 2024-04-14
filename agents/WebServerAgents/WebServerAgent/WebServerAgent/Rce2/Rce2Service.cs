using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text;
using PubSub;

namespace Rce2;

public class Rce2Service
{
    private Guid AgentId { get; }
    private string Address => $"https://localhost:7113/api/agent/{AgentId}";

    private readonly IHttpClientFactory _httpClientFactory;

    public Rce2Service(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;

        AgentId = Guid.NewGuid();
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
                        case Rce2Contacts.Ins.Render:
                            await TryRun(() => HandleRender(rce2Message));
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

    public Task HandleRequest(List<string> requestPayload)
    {
        return TryRun(async () =>
        {
            await _httpClientFactory.CreateClient().PostAsync(Address, new StringContent(JsonConvert.SerializeObject(new Rce2Message
            {
                Type = Rce2Types.StringList,
                Contact = Rce2Contacts.Outs.Request,
                Payload = JObject.FromObject(new { data = requestPayload })
            }), Encoding.UTF8, "application/json"));
        });
    }

    private async Task HandleRender(Rce2Message rce2Message)
    {
        var payloadData = rce2Message.Payload?["data"]?.ToObject<List<string>>();
        if (payloadData == null)
        {
            return;
        }

        await Hub.Default.PublishAsync(rce2Message);
    }

    private async Task HandleWhoIsMessage()
    {
        await _httpClientFactory.CreateClient().PostAsync(Address, new StringContent(JsonConvert.SerializeObject(new Rce2Message
        {
            Type = Rce2Types.WhoIs,
            Payload = JObject.FromObject(new Rce2Agent
            {
                Id = AgentId,
                Channels = new() { "web-server" },
                Name = "Web Server Agent",
                Ins = new()
                {
                    { Rce2Contacts.Ins.Render, Rce2Types.StringList },
                },
                Outs = new()
                {
                    { Rce2Contacts.Outs.Request, Rce2Types.StringList },
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
