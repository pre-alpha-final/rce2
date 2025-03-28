﻿using ChangeSpotterAgent.Rce2;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;

namespace ChangeSpotterAgent;

internal class Program
{
    private static int _x, _y;
    private static Guid AgentId = Guid.NewGuid();
    private static string Address = $"https://localhost:7113/api/agent/{AgentId}";
    private static bool IsActive = false;
    private static int ImageCount = 0;

    private static async Task Main(string[] args)
    {
        _ = Task.Run(FeedHandler);

        var oldColor = Color.Empty;
        while (true)
        {
            var color = Win32.GetPixelColor(_x, _y);
            if (IsActive && color != oldColor)
            {
                await TryRun(ChangeFound);
            }
            oldColor = color;
            await Task.Delay(500);
        }
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
                        case Rce2Contacts.Ins.Start:
                            (_x, _y) = Win32.GetCursorPosition();
                            IsActive = true;
                            break;

                        case Rce2Contacts.Ins.Stop:
                            IsActive = false;
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
                Name = "Change Spotter",
                Ins = new()
                {
                    { Rce2Contacts.Ins.Start, Rce2Types.Void },
                    { Rce2Contacts.Ins.Stop, Rce2Types.Void }
                },
                Outs = new()
                {
                    { Rce2Contacts.Outs.ChangeFound, Rce2Types.String }
                }
            }),
        }), Encoding.UTF8, "application/json"));
    }

    private static async Task ChangeFound()
    {
        if (ImageCount < 100)
        {
            var (x, y) = Win32.GetScreenSize();
            using var captureBmp = new Bitmap(x, y, PixelFormat.Format32bppArgb);
            using var captureGraphic = Graphics.FromImage(captureBmp);
            captureGraphic.CopyFromScreen(0, 0, 0, 0, captureBmp.Size);
            var timestamp = DateTimeOffset.Now;
            captureBmp.Save($"{timestamp.Ticks}_{timestamp:yyyy-dd-M--HH-mm-ss}.jpg", ImageFormat.Jpeg);
            ++ImageCount;
        }

        using var httpClient = new HttpClient();
        await httpClient.PostAsync(Address, new StringContent(JsonConvert.SerializeObject(new Rce2Message
        {
            Type = Rce2Types.String,
            Contact = Rce2Contacts.Outs.ChangeFound,
            Payload = JObject.FromObject(new { data = $"Pixel color changed - {DateTimeOffset.Now}" })
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
