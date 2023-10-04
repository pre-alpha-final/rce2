using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text;

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
                var rce2Messages = JsonConvert.DeserializeObject<List<Rce2Message>>(content);
                foreach (var rce2Message in rce2Messages)
                {
                    switch (rce2Message.Contact)
                    {
                        case Rce2Contacts.Ins.ExampleInput:
                            await TryRun(() => HandleExampleInput(rce2Message.Payload));
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

    private async Task HandleExampleInput(JToken payload)
    {
        await Task.Delay(1000);
        await HandleExampleOutput(payload["data"]?.ToObject<string>());
    }

    private Task HandleExampleOutput(string output)
    {
        return TryRun(async () =>
        {
            await _httpClientFactory.CreateClient().PostAsync(Address, new StringContent(JsonConvert.SerializeObject(new Rce2Message
            {
                Type = Rce2Types.String,
                Contact = Rce2Contacts.Outs.ExampleOutput,
                Payload = JObject.FromObject(new { data = output })
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
                Name = "Rce2 minimal boilerplate",
                Ins = new()
                {
                    { Rce2Contacts.Ins.ExampleInput, Rce2Types.String },
                },
                Outs = new()
                {
                    { Rce2Contacts.Outs.ExampleOutput, Rce2Types.String },
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
