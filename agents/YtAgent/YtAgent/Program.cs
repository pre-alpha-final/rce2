using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using YtAgent.Rce2;

namespace YtAgent;

internal class Program
{
    private static Guid AgentId = Guid.NewGuid();
    private static string Address = $"https://localhost:7113/api/agent/{AgentId}";

    private static async Task Main(string[] args)
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
                        case Rce2Contacts.Ins.Back:
                            await TryRun(() => PressNTimes(rce2Message, Keyboard.ScanCodes.J));
                            break;

                        case Rce2Contacts.Ins.PauseResume:
                            await TryRun(() => Keyboard.CustomPressKey(Keyboard.ScanCodes.K));
                            break;

                        case Rce2Contacts.Ins.Forward:
                            await TryRun(() => PressNTimes(rce2Message, Keyboard.ScanCodes.L));
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

    private static async Task PressNTimes(Rce2Message rce2Message, Keyboard.ScanCodes scanCode)
    {
        for (var i = 0; i < rce2Message.Payload["data"].ToObject<int>(); i++)
        {
            await Keyboard.CustomPressKey(scanCode);
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
                Name = "Basic YT control",
                Ins = new()
                {
                    { Rce2Contacts.Ins.Back, Rce2Types.Number },
                    { Rce2Contacts.Ins.PauseResume, Rce2Types.Number },
                    { Rce2Contacts.Ins.Forward, Rce2Types.Number },
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
