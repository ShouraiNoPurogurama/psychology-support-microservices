using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace ChatBox.API.Utils;

public class CustomUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        return connection.User.FindFirst("userId")?.Value;
    }

    public string? GetAliasId(HubConnectionContext connection)
    {
        return connection.User.FindFirst("aliasId")?.Value;
    }
}