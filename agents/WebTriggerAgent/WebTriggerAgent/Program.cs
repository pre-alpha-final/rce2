using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text;
using WebTriggerAgent.Rce2;
using System.Text.RegularExpressions;

namespace WebTriggerAgent
{
    public class Program
    {
        private static Guid AgentId = Guid.NewGuid();
        private static string Address = $"https://localhost:7113/api/agent/{AgentId}";

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var app = builder.Build();

            _ = Task.Run(FeedHandler);

            app.MapGet("/", async (HttpRequest request) =>
            {
                if (request.QueryString.HasValue == false)
                {
                    return "No trigger";
                }
                var query = request.QueryString.Value;

                await HandleChannel(query);
                await HandleAll(query);

                return $"Triggered with '{query.Substring(1, query.Length - 1)}'";
            });
            app.Run();
        }

        private static async Task HandleChannel(string? query)
        {
            var channelMatch = new Regex(@"channel=(\d+)").Match(query);
            if (channelMatch.Groups.Count <= 1)
            {
                return;
            }

            var httpClient = new HttpClient();
            await httpClient.PostAsync(Address, new StringContent(JsonConvert.SerializeObject(new Rce2Message
            {
                Type = Rce2Types.String,
                Contact = channelMatch.Groups[1].Value switch
                {
                    "0" => Rce2Contacts.Outs.Channel0,
                    "1" => Rce2Contacts.Outs.Channel1,
                    "2" => Rce2Contacts.Outs.Channel2,
                    "3" => Rce2Contacts.Outs.Channel3,
                    "4" => Rce2Contacts.Outs.Channel4,
                    "5" => Rce2Contacts.Outs.Channel5,
                    "6" => Rce2Contacts.Outs.Channel6,
                    "7" => Rce2Contacts.Outs.Channel7,
                    "8" => Rce2Contacts.Outs.Channel8,
                    "9" => Rce2Contacts.Outs.Channel9,
                    _ => throw new NotImplementedException(),
                },
                Payload = JObject.FromObject(new { data = query.Substring(1, query.Length - 1) })
            }), Encoding.UTF8, "application/json"));
        }

        private static async Task HandleAll(string? query)
        {
            var httpClient = new HttpClient();
            await httpClient.PostAsync(Address, new StringContent(JsonConvert.SerializeObject(new Rce2Message
            {
                Type = Rce2Types.String,
                Contact = Rce2Contacts.Outs.All,
                Payload = JObject.FromObject(new { data = query.Substring(1, query.Length - 1) })
            }), Encoding.UTF8, "application/json"));
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
                Payload = JObject.FromObject(new Rce2Agent
                {
                    Id = AgentId,
                    Name = "Web Trigger Agent",
                    Ins = new()
                    {
                    },
                    Outs = new()
                    {
                        { Rce2Contacts.Outs.Channel0, Rce2Types.String },
                        { Rce2Contacts.Outs.Channel1, Rce2Types.String },
                        { Rce2Contacts.Outs.Channel2, Rce2Types.String },
                        { Rce2Contacts.Outs.Channel3, Rce2Types.String },
                        { Rce2Contacts.Outs.Channel4, Rce2Types.String },
                        { Rce2Contacts.Outs.Channel5, Rce2Types.String },
                        { Rce2Contacts.Outs.Channel6, Rce2Types.String },
                        { Rce2Contacts.Outs.Channel7, Rce2Types.String },
                        { Rce2Contacts.Outs.Channel8, Rce2Types.String },
                        { Rce2Contacts.Outs.Channel9, Rce2Types.String },
                        { Rce2Contacts.Outs.All, Rce2Types.String },
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
}