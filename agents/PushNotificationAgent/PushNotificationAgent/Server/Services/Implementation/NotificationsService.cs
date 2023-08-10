using PushNotificationAgent.Client.Infrastructure;
using System.Collections.Concurrent;

namespace PushNotificationAgent.Server.Services.Implementation;

public class NotificationsService : INotificationsService
{
    private ConcurrentBag<NotificationSubscription> _notificationSubscriptions = new();

    public List<NotificationSubscription> GetAll()
    {
        return _notificationSubscriptions.ToList();
    }

    public NotificationSubscription Add(NotificationSubscription notificationSubscription)
    {
        _notificationSubscriptions.Add(notificationSubscription);

        return notificationSubscription;
    }
}
