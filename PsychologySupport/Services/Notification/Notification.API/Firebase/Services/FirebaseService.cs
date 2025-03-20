using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Notification.API.Firebase.ServiceContracts;

namespace Notification.API.Firebase.Services;

public class FirebaseService(ILogger<FirebaseService> logger) : IFirebaseService
{
    public async Task SendPushNotification(string FCMToken, string subject, string body)
    {
        FirebaseApp.Create(new AppOptions()
        {
            Credential = GoogleCredential.FromFile("firebase_key.json")
        });

        var message = new Message
        {
            Data = new Dictionary<string, string>()
            {
                { $"{subject}", $"{body}" }
            },
            Token = FCMToken,
            Notification = new FirebaseAdmin.Messaging.Notification()
            {
                Title = "Test from NA",
                Body = "Hello"
            }
        };

        string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
        logger.LogInformation("Successfully sent message: {Response}", response);
    }
}