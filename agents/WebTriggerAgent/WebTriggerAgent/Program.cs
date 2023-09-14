using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text;
using WebTriggerAgent.Rce2;

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

                using var httpClient = new HttpClient();
                await httpClient.PostAsync(Address, new StringContent(JsonConvert.SerializeObject(new Rce2Message
                {
                    Type = Rce2Types.String,
                    Contact = Rce2Contacts.Outs.Trigger,
                    Payload = JObject.FromObject(new { data = query.Substring(1, query.Length - 1) })
                }), Encoding.UTF8, "application/json"));

                return $"Triggered with '{query.Substring(1, query.Length - 1)}'";
            });
            app.Run();
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
                        { Rce2Contacts.Outs.Trigger, Rce2Types.String }
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