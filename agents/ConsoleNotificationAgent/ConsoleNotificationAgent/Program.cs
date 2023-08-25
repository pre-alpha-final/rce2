using ConsoleNotificationAgent.Rce2;
using NAudio.Wave;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace ConsoleNotificationAgent;

public class Program
{
    private const string NotificationSoundPath = "MP3_PATH";
    private static Guid AgentId = Guid.NewGuid();
    private static string Address = $"https://localhost:7113/api/agent/{AgentId}";

    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll")]
    static extern bool FlashWindow(IntPtr hwnd, bool bInvert);

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
                        case Rce2Contacts.Ins.Alert:
                            await TryRun(() => HandleAlertMessage(rce2Message.Payload));
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

    private static async Task HandleWhoIsMessage()
    {
        using var httpClient = new HttpClient();
        await httpClient.PostAsync(Address, new StringContent(JsonConvert.SerializeObject(new Rce2Message
        {
            Type = Rce2Types.WhoIs,
            Payload = JObject.FromObject(new Rce2Agent
            {
                Id = AgentId,
                Name = "Console Notification Agent",
                Ins = new()
                {
                    { Rce2Contacts.Ins.Alert, Rce2Types.String }
                },
                Outs = new()
                {
                }
            }),
        }), Encoding.UTF8, "application/json"));
    }

    private static async Task HandleAlertMessage(JToken payload)
    {
        Console.WriteLine($"[{DateTime.Now.ToString("g")}] {payload["data"].ToObject<string>()}");

        FlashWindow(FindWindow(null, Console.Title), true);

        var waveOutEvent = new WaveOutEvent();
        waveOutEvent.Init(new Mp3FileReader(NotificationSoundPath));
        waveOutEvent.Pause();
        waveOutEvent.Volume = 0.25f;
        waveOutEvent.Play();
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
