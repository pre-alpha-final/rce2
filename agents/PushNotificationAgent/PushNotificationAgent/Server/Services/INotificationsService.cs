﻿using PushNotificationAgent.Client.Infrastructure;

namespace PushNotificationAgent.Server.Services;

public interface INotificationsService
{
    public List<NotificationSubscription> GetAll();
    public NotificationSubscription Add(NotificationSubscription notificationSubscription);
}
