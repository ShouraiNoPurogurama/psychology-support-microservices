using MediatR;
using Notification.API.Features.Firebase.Contracts;

namespace Notification.API.Features.Firebase.SendPushNotification;

public class SendMobilePushNotificationEventHandler(IFirebaseService firebaseService) : INotificationHandler<SendMobilePushNotificationEvent>
{
    public async Task Handle(SendMobilePushNotificationEvent notification, CancellationToken cancellationToken)
    {
        await firebaseService.SendPushNotification(notification.FCMToken, notification.Subject, notification.Body);
    }
}