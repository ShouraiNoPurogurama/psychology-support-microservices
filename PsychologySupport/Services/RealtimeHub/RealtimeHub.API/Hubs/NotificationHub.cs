using Microsoft.AspNetCore.SignalR;

namespace RealtimeHub.API.Hubs;

/// <summary>
/// SignalR Hub for real-time notification delivery
/// </summary>
public class NotificationHub : Hub<INotificationHubClient>
{
    private static readonly Dictionary<string, string> _userConnections = new();
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var aliasId = Context.GetHttpContext()?.Request.Query["aliasId"].ToString();
        
        if (!string.IsNullOrEmpty(aliasId))
        {
            _userConnections[aliasId] = Context.ConnectionId;
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{aliasId}");
            
            _logger.LogInformation(
                "User {AliasId} connected with connection {ConnectionId}",
                aliasId, Context.ConnectionId);
        }
        else
        {
            _logger.LogWarning(
                "Connection {ConnectionId} attempted without aliasId",
                Context.ConnectionId);
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
            
            _logger.LogInformation(
                "User {AliasId} disconnected from connection {ConnectionId}",
                aliasId, Context.ConnectionId);
        }

        if (exception != null)
        {
            _logger.LogError(exception,
                "Connection {ConnectionId} disconnected with error",
                Context.ConnectionId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Allows a client to explicitly join a user group
    /// </summary>
    public async Task JoinUserGroup(string aliasId)
    {
        _userConnections[aliasId] = Context.ConnectionId;
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{aliasId}");
        
        _logger.LogInformation(
            "User {AliasId} joined group with connection {ConnectionId}",
            aliasId, Context.ConnectionId);
    }

    /// <summary>
    /// Allows a client to explicitly leave a user group
    /// </summary>
    public async Task LeaveUserGroup(string aliasId)
    {
        _userConnections.Remove(aliasId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{aliasId}");
        
        _logger.LogInformation(
            "User {AliasId} left group from connection {ConnectionId}",
            aliasId, Context.ConnectionId);
    }

    /// <summary>
    /// Gets the connection ID for a specific user (static method for internal use)
    /// </summary>
    public static string? GetConnectionId(string aliasId)
    {
        return _userConnections.TryGetValue(aliasId, out var connectionId) ? connectionId : null;
    }

    /// <summary>
    /// Gets the total number of active connections
    /// </summary>
    public static int GetActiveConnectionCount()
    {
        return _userConnections.Count;
    }

    /// <summary>
    /// Checks if a user is currently connected
    /// </summary>
    public static bool IsUserConnected(string aliasId)
    {
        return _userConnections.ContainsKey(aliasId);
    }
}
