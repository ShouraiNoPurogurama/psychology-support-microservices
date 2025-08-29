namespace Notification.API.Domains.Firebase.ServiceContracts;

public interface IFirebaseService
{
    Task SendPushNotification(IEnumerable<string> FCMTokens, string subject, string body);
}