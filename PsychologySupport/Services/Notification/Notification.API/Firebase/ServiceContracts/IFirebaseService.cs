namespace Notification.API.Firebase.ServiceContracts;

public interface IFirebaseService
{
    Task SendPushNotification(string FCMToken, string subject, string body);
}