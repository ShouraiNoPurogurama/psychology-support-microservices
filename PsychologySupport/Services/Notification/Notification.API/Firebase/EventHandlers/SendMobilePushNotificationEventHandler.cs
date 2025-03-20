using MediatR;
using Notification.API.Firebase.Events;
using Notification.API.Firebase.ServiceContracts;

namespace Notification.API.Firebase.EventHandlers;

public class SendMobilePushNotificationEventHandler(IFirebaseService firebaseService) : INotificationHandler<SendMobilePushNotificationEvent>
{
    public Task Handle(SendMobilePushNotificationEvent notification, CancellationToken cancellationToken)
    {
        firebaseService.SendPushNotification(notification.FCMToken, notification.Subject, notification.Body);
        
        return Task.CompletedTask;
    }
}