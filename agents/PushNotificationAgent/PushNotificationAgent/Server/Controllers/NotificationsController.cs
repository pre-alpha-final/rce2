using Microsoft.AspNetCore.Mvc;
using PushNotificationAgent.Client.Infrastructure;
using PushNotificationAgent.Server.Services;

namespace PushNotificationAgent.Server.Controllers;

[Route("notifications")]
[ApiController]
public class NotificationsController
{
    private readonly INotificationsService _notificationsService;

    public NotificationsController(INotificationsService notificationsService)
    {
        _notificationsService = notificationsService;
    }

    [HttpPut("subscribe")]
    public NotificationSubscription Subscribe(NotificationSubscription subscription)
    {
        _notificationsService.Add(subscription);
        return subscription;
    }
}
