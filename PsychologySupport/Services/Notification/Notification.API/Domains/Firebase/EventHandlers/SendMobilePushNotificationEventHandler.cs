using MediatR;
using Notification.API.Domains.Firebase.Events;
using Notification.API.Domains.Firebase.ServiceContracts;

namespace Notification.API.Domains.Firebase.EventHandlers;

public class SendMobilePushNotificationEventHandler(IFirebaseService firebaseService) : INotificationHandler<SendMobilePushNotificationEvent>
{
    public async Task Handle(SendMobilePushNotificationEvent notification, CancellationToken cancellationToken)
    {
        await firebaseService.SendPushNotification(notification.FCMToken, notification.Subject, notification.Body);
    }
}