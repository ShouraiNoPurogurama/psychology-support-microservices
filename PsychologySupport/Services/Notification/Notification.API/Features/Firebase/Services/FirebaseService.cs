using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Notification.API.Features.Firebase.Contracts;

namespace Notification.API.Features.Firebase.Services;

public class FirebaseService(ILogger<FirebaseService> logger) : IFirebaseService
{
    public async Task SendPushNotification(IEnumerable<string> FCMTokens, string subject, string body)
    {
        if (FirebaseApp.DefaultInstance == null)
        {
            FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromFile("firebase_key.json")
            });
        }
        List<Message> messages = [];

        var messageTemplate = new Message
        {
            Data = new Dictionary<string, string>()
            {
                { $"{subject}", $"{body}" }
            },
            Notification = new FirebaseAdmin.Messaging.Notification()
            {
                Title = $"{subject}",
                Body =  $"{body}"
            }
        };

        foreach (var token in FCMTokens)
        {
            messages.Add(new Message
            {
                Data = messageTemplate.Data,
                Notification = messageTemplate.Notification,
                Token = token
            });
        }

        // var response = await FirebaseMessaging.DefaultInstance.SendEachAsync(messages);
        // logger.LogInformation("Successfully sent message: {Response}", response);
    }
}