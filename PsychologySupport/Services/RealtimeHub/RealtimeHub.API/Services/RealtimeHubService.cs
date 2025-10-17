using Microsoft.AspNetCore.SignalR;
using RealtimeHub.API.Hubs;
using RealtimeHub.API.Models;

namespace RealtimeHub.API.Services;

/// <summary>
/// Service for sending real-time notifications and messages via SignalR
/// </summary>
public class RealtimeHubService : IRealtimeHubService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<RealtimeHubService> _logger;

    public RealtimeHubService(
        IHubContext<NotificationHub> hubContext,
        ILogger<RealtimeHubService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendNotificationToUserAsync(
        Guid aliasId,
        NotificationMessage notification,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _hubContext.Clients
                .Group($"user_{aliasId}")
                .SendAsync("ReceiveNotification", notification, cancellationToken);

            _logger.LogInformation(
                "Sent notification {NotificationId} to user {AliasId} via SignalR",
                notification.Id, aliasId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to send notification {NotificationId} to user {AliasId} via SignalR",
                notification.Id, aliasId);
            throw;
        }
    }

    public async Task SendNotificationToUsersAsync(
        List<Guid> aliasIds,
        NotificationMessage notification,
        CancellationToken cancellationToken = default)
    {
        var tasks = aliasIds.Select(aliasId => 
            SendNotificationToUserAsync(aliasId, notification, cancellationToken));
        
        await Task.WhenAll(tasks);
    }

    public async Task SendMessageToUserAsync(
        Guid aliasId,
        string method,
        object message,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _hubContext.Clients
                .Group($"user_{aliasId}")
                .SendAsync(method, message, cancellationToken);

            _logger.LogInformation(
                "Sent message via method {Method} to user {AliasId}",
                method, aliasId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to send message via method {Method} to user {AliasId}",
                method, aliasId);
            throw;
        }
    }

    public async Task SendMessageToGroupAsync(
        string groupName,
        string method,
        object message,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _hubContext.Clients
                .Group(groupName)
                .SendAsync(method, message, cancellationToken);

            _logger.LogInformation(
                "Sent message via method {Method} to group {GroupName}",
                method, groupName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to send message via method {Method} to group {GroupName}",
                method, groupName);
            throw;
        }
    }

    public int GetActiveConnectionCount()
    {
        return NotificationHub.GetActiveConnectionCount();
    }

    public bool IsUserConnected(Guid aliasId)
    {
        return NotificationHub.IsUserConnected(aliasId.ToString());
    }
}
