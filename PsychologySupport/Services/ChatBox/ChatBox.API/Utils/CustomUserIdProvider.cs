using Microsoft.AspNetCore.SignalR;

namespace ChatBox.API.Utils;

public class CustomUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        return connection.User.FindFirst("userId")?.Value;
    }
}