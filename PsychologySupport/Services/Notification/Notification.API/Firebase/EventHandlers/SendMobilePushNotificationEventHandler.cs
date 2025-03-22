using MediatR;
using Notification.API.Firebase.Events;
using Notification.API.Firebase.ServiceContracts;

namespace Notification.API.Firebase.EventHandlers;

public class SendMobilePushNotificationEventHandler(IFirebaseService firebaseService) : INotificationHandler<SendMobilePushNotificationEvent>
{
    public async Task Handle(SendMobilePushNotificationEvent notification, CancellationToken cancellationToken)
    {
        await firebaseService.SendPushNotification(notification.FCMToken, notification.Subject, notification.Body);
    }
}