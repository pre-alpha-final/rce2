using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text;
using PubSub;

namespace Rce2;

public class Rce2Service
{
    private string _brokerAddress;
    private Guid _agentId;
    private string _agentName;
    private List<string> _channels;
    private Dictionary<string, string> _inputDefinitions = new();
    private Dictionary<string, string> _outputDefinitions = new();

    private readonly IHttpClientFactory _httpClientFactory;

    public Rce2Service(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public Rce2Service SetBrokerAddress(string brokerAddress)
    {
        _brokerAddress = brokerAddress;
        return this;
    }

    public Rce2Service SetAgentId(Guid agentId)
    {
        _agentId = agentId;
        return this;
    }

    public Rce2Service SetAgentName(string agentName)
    {
        _agentName = agentName;
        return this;
    }

    public Rce2Service SetChannels(List<string> channels)
    {
        _channels = channels;
        return this;
    }

    public Rce2Service SetInputDefinitions(Dictionary<string, string> inputDefinitions)
    {
        _inputDefinitions = inputDefinitions;
        return this;
    }

    public Rce2Service SetOutputDefinitions(Dictionary<string, string> outputDefinitions)
    {
        _outputDefinitions = outputDefinitions;
        return this;
    }

    public void Run()
    {
        Task.Run(FeedHandler);
    }

    public async Task Send(string contact, string rce2Type, object payload)
    {
        await _httpClientFactory.CreateClient().PostAsync($"{_brokerAddress}/api/agent/{_agentId}", new StringContent(JsonConvert.SerializeObject(new Rce2Message
        {
            Type = rce2Type,
            Contact = contact,
            Payload = JObject.FromObject(new { data = payload })
        }), Encoding.UTF8, "application/json"));
    }

    private async Task FeedHandler()
    {
        while (true)
        {
            try
            {
                var feed = await _httpClientFactory.CreateClient().GetAsync($"{_brokerAddress}/api/agent/{_agentId}");
                var content = await feed.Content.ReadAsStringAsync();
                var rce2Messages = JsonConvert.DeserializeObject<List<Rce2Message>>(content);
                foreach (var rce2Message in rce2Messages)
                {
                    if (rce2Message.Type == Rce2Types.WhoIs)
                    {
                        await TryRun(HandleWhoIsMessage);
                        continue;
                    }

                    await Hub.Default.PublishAsync(rce2Message);
                }
            }
            catch (Exception e)
            {
                await Task.Delay(1000);
                // ignore
            }
        }
    }

    private async Task HandleWhoIsMessage()
    {
        await _httpClientFactory.CreateClient().PostAsync($"{_brokerAddress}/api/agent/{_agentId}", new StringContent(JsonConvert.SerializeObject(new Rce2Message
        {
            Type = Rce2Types.WhoIs,
            Payload = JObject.FromObject(new Rce2Agent
            {
                Id = _agentId,
                Channels = _channels,
                Name = _agentName,
                Ins = _inputDefinitions,
                Outs = _outputDefinitions,
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
