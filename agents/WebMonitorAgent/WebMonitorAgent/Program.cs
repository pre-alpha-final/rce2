using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using WebMonitorAgent.Rce2;

namespace WebMonitorAgent;

internal class Program
{
    private static Guid AgentId = Guid.NewGuid();
    private static string Address = $"https://localhost:7113/api/agent/{AgentId}";

    private static void Main(string[] args)
    {
        _ = Task.Run(FeedHandler);
        _ = Task.Run(WebMonitor);
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
                        default:
                            if (rce2Message.Type == Rce2Types.WhoIs)
                            {
                                await HandleWhoIsMessage();
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
            Payload = JObject.FromObject(new Rce2Agent
            {
                Id = AgentId,
                Name = "Web Monitor",
                Ins = new()
                {
                },
                Outs = new()
                {
                    { Rce2Contacts.Outs.RaiseAlert, Rce2Types.String }
                }
            }),
        }), Encoding.UTF8, "application/json"));
    }

    private static async Task WebMonitor()
    {
        while (true)
        {
            try
            {
                var htmlDocument = new HtmlWeb().Load("https://google.com");
                var body = htmlDocument.DocumentNode.SelectSingleNode("//body").InnerHtml;

                if (body.Contains("foo"))
                {
                    await RaiseAlert("google foo");
                }
            }
            catch (Exception e)
            {
            }
            await Task.Delay(5000);
        }
    }

    private static async Task RaiseAlert(string alert)
    {
        using var httpClient = new HttpClient();
        await httpClient.PostAsync(Address, new StringContent(JsonConvert.SerializeObject(new Rce2Message
        {
            Type = Rce2Types.String,
            Contact = Rce2Contacts.Outs.RaiseAlert,
            Payload = JObject.FromObject(new
            {
                data = alert,
            }),
        }), Encoding.UTF8, "application/json"));
    }
}
