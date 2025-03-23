namespace Notification.API.Firebase.ServiceContracts;

public interface IFirebaseService
{
    Task SendPushNotification(IEnumerable<string> FCMTokens, string subject, string body);
}