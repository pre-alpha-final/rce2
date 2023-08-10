using Newtonsoft.Json;
using PushNotificationAgent.Client;
using WebPush;

namespace PushNotificationAgent.Server.Services.Implementation;

public class Rce2Service : IRce2Service
{
    private readonly INotificationsService _notificationsService;

    public Rce2Service(INotificationsService notificationsService)
    {
        _notificationsService = notificationsService;
    }

    public async Task Run()
    {
        while (true)
        {
            await Task.Delay(5000);
            foreach(var notificationSubscription in _notificationsService.GetAll())
            {
                await SendNotificationAsync(notificationSubscription, "foo\nbar");
            }
        }
    }

    private static async Task SendNotificationAsync(NotificationSubscription notificationSubscription, string message)
    {
        var publicKey = "BFox_8uFgdFdEOQSdeIvn1f_-xK2PB0AYwptmxY2PHCl-wRHZXkx8hCp4khDY4IexzoqlY1msDwJHhPQIAlIfXU";
        //var privateKey = "";

        var pushSubscription = new PushSubscription(notificationSubscription.Url, notificationSubscription.P256dh, notificationSubscription.Auth);
        var vapidDetails = new VapidDetails("mailto:<example@example.com>", publicKey, privateKey);
        var webPushClient = new WebPushClient();
        try
        {
            var payload = JsonConvert.SerializeObject(new
            {
                message,
                url = $"google.com",
            });
            await webPushClient.SendNotificationAsync(pushSubscription, payload, vapidDetails);
        }
        catch (Exception e)
        {
            // ignore
        }
    }
}
