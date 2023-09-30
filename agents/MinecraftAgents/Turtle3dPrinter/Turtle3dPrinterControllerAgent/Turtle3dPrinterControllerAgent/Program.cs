using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using Turtle3dPrinterControllerAgent.Rce2;

namespace Turtle3dPrinterControllerAgent;

public class Program
{
    static void Main(string[] args)
    {
        var builder = new HostBuilder()
            .ConfigureServices((hostContext, services) =>
            {
                services.AddHttpClient();
                services.AddSingleton<AppSettings>();
                services.AddTransient<Printer>();
                services.AddTransient<Turtle>();
            });
        var host = builder.Build();

        _ = Task.Run(() => FeedHandler(host.Services.GetRequiredService<AppSettings>()));
        _ = Task.Run(host.Services.GetRequiredService<Printer>().Run);

        Console.ReadLine();
    }

    private static async Task FeedHandler(AppSettings appSettings)
    {
        using var httpClient = new HttpClient();
        while (true)
        {
            try
            {
                var feed = await httpClient.GetAsync(appSettings.Address);
                var content = await feed.Content.ReadAsStringAsync();
                var rce2Messages = JsonConvert.DeserializeObject<List<Rce2Message>>(content);
                foreach (var rce2Message in rce2Messages)
                {
                    switch (rce2Message.Contact)
                    {
                        default:
                            if (rce2Message.Type == Rce2Types.WhoIs)
                            {
                                await TryRun(() => HandleWhoIsMessage(appSettings.AgentId, appSettings.Address));
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

    private static async Task HandleWhoIsMessage(Guid agentId, string address)
    {
        using var httpClient = new HttpClient();
        await httpClient.PostAsync(address, new StringContent(JsonConvert.SerializeObject(new Rce2Message
        {
            Type = Rce2Types.WhoIs,
            Payload = JObject.FromObject(new Rce2Agent
            {
                Id = agentId,
                Name = "Turtle 3d Printer Controller Agent",
                Ins = new()
                {
                },
                Outs = new()
                {
                    { Rce2Contacts.Outs.SendCommand, Rce2Types.String }
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
