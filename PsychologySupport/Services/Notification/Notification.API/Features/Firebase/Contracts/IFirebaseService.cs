namespace Notification.API.Features.Firebase.Contracts;

public interface IFirebaseService
{
    Task SendPushNotification(IEnumerable<string> FCMTokens, string subject, string body);
}