using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text;

namespace Rce2;

public class Rce2Service
{
    private Guid AgentId { get; }
    private string Address => $"https://localhost:7113/api/agent/{AgentId}";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IServiceProvider _serviceProvider;

    public Rce2Service(IHttpClientFactory httpClientFactory, IServiceProvider serviceProvider)
    {
        _httpClientFactory = httpClientFactory;
        _serviceProvider = serviceProvider;

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
                        case Rce2Contacts.Ins.Request:
                            await TryRun(() => HandleRequest(rce2Message.Payload, (WwwHandler)_serviceProvider.GetService(typeof(WwwHandler))!));
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

    private async Task HandleRequest(JToken? payload, WwwHandler wwwHandler)
    {
        var payloadData = payload?["data"]?.ToObject<List<string>>();
        if (payloadData == null)
        {
            return;
        }

        if (payloadData.GetValue("rce2_agent_id") != AgentId.ToString())
        {
            return;
        }

        var renderResponse = await wwwHandler.Handle(payloadData);
        await HandleRender(renderResponse);
    }

    private Task HandleRender(List<string> requestPayload)
    {
        return TryRun(async () =>
        {
            await _httpClientFactory.CreateClient().PostAsync(Address, new StringContent(JsonConvert.SerializeObject(new Rce2Message
            {
                Type = Rce2Types.StringList,
                Contact = Rce2Contacts.Outs.Render,
                Payload = JObject.FromObject(new { data = requestPayload })
            }), Encoding.UTF8, "application/json"));
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
                Channels = new() { "web-server" },
                Name = "Hosting Agent",
                Ins = new()
                {
                    { Rce2Contacts.Ins.Request, Rce2Types.StringList },
                },
                Outs = new()
                {
                    { Rce2Contacts.Outs.Render, Rce2Types.StringList },
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
