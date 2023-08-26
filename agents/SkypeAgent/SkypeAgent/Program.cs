using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using SkypeAgent.Rce2;
using System.Text;

namespace SkypeAgent;

public class Program
{
    private static Guid AgentId = Guid.NewGuid();
    private static string Address = $"https://localhost:7113/api/agent/{AgentId}";

    private const string AgentSkypeUser = "";
    private const string AgentSkypePassword = "";
    private const string RecipientId = "";
    private const string SendMessageScript =
@"
import sys
from skpy import Skype

skype = Skype(sys.argv[1], sys.argv[2])
skype.contacts[sys.argv[3]].chat.sendMsg(sys.argv[4])
print(""sent "" + sys.argv[4])
";

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
                        case Rce2Contacts.Ins.Send:
                            await TryRun(() => HandleSendMessage(rce2Message.Payload));
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

    private static async Task HandleSendMessage(JToken payload)
    {
        await new PythonRunner().Run(SendMessageScript,
            $"\"{AgentSkypeUser}\" \"{AgentSkypePassword}\" \"{RecipientId}\" \"{payload["data"].ToObject<string>()}\"");
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
                Name = "Skype Agent",
                Ins = new()
                {
                    { Rce2Contacts.Ins.Send, Rce2Types.String }
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
