using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System;
using System.Threading.Tasks;

namespace Rpzw
{
    internal class Program
    {
        private static Guid AgentId = Guid.NewGuid();
        private static string Address = $"https://localhost:7113/api/agent/{AgentId}";

        static void Main(string[] args)
        {
            _ = Task.Run(FeedHandler);

            var gpioService = new GpioService();
            gpioService.Run(Risen);

            Console.ReadKey(true);
        }

        private static async Task FeedHandler()
        {
            using (var httpClient = new HttpClient())
            {
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
        }

        private static async Task HandleWhoIsMessage()
        {
            using (var httpClient = new HttpClient())
            {
                await httpClient.PostAsync(Address, new StringContent(JsonConvert.SerializeObject(new Rce2Message
                {
                    Type = Rce2Types.WhoIs,
                    Payload = JObject.FromObject(new Agent
                    {
                        Id = AgentId,
                        Name = "Rpzw",
                        Ins = new Dictionary<string, string>()
                        {
                        },
                        Outs = new Dictionary<string, string>()
                        {
                            { "button_pressed", Rce2Types.Void }
                        }
                    }),
                }), Encoding.UTF8, "application/json"));
            }
        }

        private static void Risen(int pin, bool rising)
        {
            if ((pin == 13 && rising == true) == false)
            {
                return;
            }

            try
            {
                Task.Run(async () =>
                {
                    using (var httpClient = new HttpClient())
                    {
                        await httpClient.PostAsync(Address, new StringContent(JsonConvert.SerializeObject(new Rce2Message
                        {
                            Type = Rce2Types.StringList,
                            Contact = "button_pressed",
                            Payload = JToken.Parse("{}"),
                        }), Encoding.UTF8, "application/json"));
                    }
                });
                return;
            }
            catch (Exception e)
            {
                // ignore
            }
        }
    }
}
