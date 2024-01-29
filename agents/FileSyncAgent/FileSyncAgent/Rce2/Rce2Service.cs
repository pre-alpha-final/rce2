using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text;
using FileSyncAgent.Infrastructure;

namespace Rce2;

public class Rce2Service
{
    private Guid AgentId { get; }
    private string Address => $"https://localhost:7113/api/agent/{AgentId}";

    private readonly IHttpClientFactory _httpClientFactory;
    private Func<List<SyncMetadata>, string, Task>? _updateReceivedCallback = null;

    public Rce2Service(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;

        AgentId = Guid.NewGuid();
    }

    public void Run()
    {
        Task.Run(FeedHandler);
    }

    public void SetUpdateReceivedCallback(Func<List<SyncMetadata>, string, Task> updateReceivedCallback)
    {
        _updateReceivedCallback = updateReceivedCallback;
    }

    public Task SendUpdate(FileSyncPayload fileSyncPayload)
    {
        return TryRun(async () =>
        {
            await _httpClientFactory.CreateClient().PostAsync(Address, new StringContent(JsonConvert.SerializeObject(new Rce2Message
            {
                Type = Rce2Types.Custom,
                Contact = Rce2Contacts.Outs.SendUpdate,
                Payload = JObject.FromObject(new { data = fileSyncPayload })
            }), Encoding.UTF8, "application/json"));
        });
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
                        case Rce2Contacts.Ins.UpdateReceived:
                            await TryRun(() => HandleUpdateReceived(rce2Message.Payload));
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

    private async Task HandleUpdateReceived(JToken payload)
    {
        if (_updateReceivedCallback != null)
        {
            var fileSyncPayload = payload["data"]?.ToObject<FileSyncPayload>();
            await _updateReceivedCallback(fileSyncPayload.Syncs, fileSyncPayload.Data);
        }
    }

    private async Task HandleWhoIsMessage()
    {
        await _httpClientFactory.CreateClient().PostAsync(Address, new StringContent(JsonConvert.SerializeObject(new Rce2Message
        {
            Type = Rce2Types.WhoIs,
            Payload = JObject.FromObject(new Rce2Agent
            {
                Id = AgentId,
                Name = "File Sync Agent",
                Ins = new()
                {
                    { Rce2Contacts.Ins.UpdateReceived, Rce2Types.Custom },
                },
                Outs = new()
                {
                    { Rce2Contacts.Outs.SendUpdate, Rce2Types.Custom },
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
