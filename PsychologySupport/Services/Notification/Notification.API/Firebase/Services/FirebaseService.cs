using FirebaseAdmin.Messaging;

namespace Notification.API.Firebase.Services;

public class FirebaseService(ILogger<FirebaseService> logger)
{
    public async Task SendPushNotification()
    {
        var registrationToken = "BKPvgb5J0aXgS8QPPdctEGFtGw3WCUafYbQKQHWkev4UxjnpuVtQTksH3HBX7zMUNnz_Rv_S_UFDdzNlRYAUWW8";

        var message = new Message
        {
            Data = new Dictionary<string, string>()
            {
                { "MyData", "hehehehehehe" }
            },
            Token = registrationToken,
            Notification = new FirebaseAdmin.Messaging.Notification()
            {
                Title = "Test from NA",
                Body = "Hello"
            }
        };

        string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
        
        logger.LogDebug($"Successfully sent message: {response}");
    }
}