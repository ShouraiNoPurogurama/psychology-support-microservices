using Microsoft.AspNetCore.SignalR;

namespace Notification.API.Hubs;

public class NotificationHub : Hub
{
    private static readonly Dictionary<string, string> _userConnections = new();

    public override async Task OnConnectedAsync()
    {
        var aliasId = Context.GetHttpContext()?.Request.Query["aliasId"].ToString();
        
        if (!string.IsNullOrEmpty(aliasId))
        {
            _userConnections[aliasId] = Context.ConnectionId;
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{aliasId}");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var aliasId = _userConnections.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;
        
        if (!string.IsNullOrEmpty(aliasId))
        {
            _userConnections.Remove(aliasId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{aliasId}");
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinUserGroup(string aliasId)
    {
        _userConnections[aliasId] = Context.ConnectionId;
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{aliasId}");
    }

    public async Task LeaveUserGroup(string aliasId)
    {
        _userConnections.Remove(aliasId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{aliasId}");
    }

    public static string? GetConnectionId(string aliasId)
    {
        return _userConnections.TryGetValue(aliasId, out var connectionId) ? connectionId : null;
    }
}
