using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System;
using System.Threading.Tasks;
using RpzwAgent.Rce2;

namespace RpzwAgent
{
    internal class Program
    {
        private static Guid AgentId = Guid.NewGuid();
        private static string Address = $"https://localhost:7113/api/agent/{AgentId}";

        private static GpioService _gpioService;

        static void Main(string[] args)
        {
            _ = Task.Run(FeedHandler);

            _gpioService = new GpioService();
            _gpioService.Run(OnPinStateChange);

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
                                case Rce2Contacts.Ins.Trigger1:
                                    await TryRun(() => HandleTrigger1(rce2Message.Payload));
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
        }

        private static async Task HandleTrigger1(JToken payload)
        {
            _gpioService.SetPin(18, payload["data"].ToObject<bool>());
        }

        private static async Task HandleWhoIsMessage()
        {
            using (var httpClient = new HttpClient())
            {
                await httpClient.PostAsync(Address, new StringContent(JsonConvert.SerializeObject(new Rce2Message
                {
                    Type = Rce2Types.WhoIs,
                    Payload = JObject.FromObject(new Rce2Agent
                    {
                        Id = AgentId,
                        Name = "Rpzw",
                        Ins = new Dictionary<string, string>()
                        {
                            { Rce2Contacts.Ins.Trigger1, Rce2Types.Boolean }
                        },
                        Outs = new Dictionary<string, string>()
                        {
                            { Rce2Contacts.Outs.ButtonPressed, Rce2Types.Number }
                        }
                    }),
                }), Encoding.UTF8, "application/json"));
            }
        }

        private static void OnPinStateChange(int pin, bool isRising)
        {
            if (isRising == false)
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
                            Type = Rce2Types.Number,
                            Contact = Rce2Contacts.Outs.ButtonPressed,
                            Payload = JObject.FromObject(new { data = pin }),
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
