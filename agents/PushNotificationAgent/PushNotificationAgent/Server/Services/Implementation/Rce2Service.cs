using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PushNotificationAgent.Client.Infrastructure;
using PushNotificationAgent.Server.Rce2;
using System.Text;
using WebPush;

namespace PushNotificationAgent.Server.Services.Implementation;

public class Rce2Service : IRce2Service
{
    private const string PublicKey = "BK7i6MrOsXEix-APsqPX568FeOzqUdxb_FoWKw-vjqz9WTIOqI6_LQzS6Uft_-vC1cnD7nS3iw-Z7_oizRrpO38";
    //private const string PrivateKey = "";

    private static Guid AgentId = Guid.NewGuid();
    private static string Address = $"https://localhost:7113/api/agent/{AgentId}";

    private readonly INotificationsService _notificationsService;

    public Rce2Service(INotificationsService notificationsService)
    {
        _notificationsService = notificationsService;
    }

    public async Task Run()
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
                        case Rce2Contacts.Ins.Push:
                            await HandlePush(rce2Message.Payload);
                            break;

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

    private async Task HandlePush(JToken payload)
    {
        foreach (var notificationSubscription in _notificationsService.GetAll())
        {
            await SendNotificationAsync(notificationSubscription, payload["data"].ToObject<string>());
        }
    }

    private async Task HandleWhoIsMessage()
    {
        using var httpClient = new HttpClient();
        await httpClient.PostAsync(Address, new StringContent(JsonConvert.SerializeObject(new Rce2Message
        {
            Type = Rce2Types.WhoIs,
            Payload = JObject.FromObject(new Rce2Agent
            {
                Id = AgentId,
                Name = "Rce2Push",
                Ins = new()
                {
                    { Rce2Contacts.Ins.Push, Rce2Types.String }
                },
                Outs = new()
                {
                }
            }),
        }), Encoding.UTF8, "application/json"));
    }

    private static async Task SendNotificationAsync(NotificationSubscription notificationSubscription, string message)
    {
        var pushSubscription = new PushSubscription(notificationSubscription.Url, notificationSubscription.P256dh, notificationSubscription.Auth);
        var vapidDetails = new VapidDetails("mailto:<example@example.com>", PublicKey, PrivateKey);
        var webPushClient = new WebPushClient();
        try
        {
            var payload = JsonConvert.SerializeObject(new
            {
                message,
                url = $"",
            });
            await webPushClient.SendNotificationAsync(pushSubscription, payload, vapidDetails);
        }
        catch (Exception e)
        {
            // ignore
        }
    }
}
