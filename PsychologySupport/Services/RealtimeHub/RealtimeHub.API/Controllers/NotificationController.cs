using Carter;
using Microsoft.AspNetCore.Mvc;
using RealtimeHub.API.Models;
using RealtimeHub.API.Services;

namespace RealtimeHub.API.Controllers;

/// <summary>
/// REST API endpoints for triggering real-time notifications
/// This allows other services to send real-time notifications via HTTP
/// </summary>
public class NotificationEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/realtime")
            .WithTags("Realtime Notifications");

        // Send notification to a single user
        group.MapPost("/send-to-user", async (
            [FromBody] SendNotificationRequest request,
            [FromServices] IRealtimeHubService hubService,
            CancellationToken ct) =>
        {
            await hubService.SendNotificationToUserAsync(
                request.AliasId,
                request.Notification,
                ct);

            return Results.Ok(new { success = true, message = "Notification sent" });
        })
        .WithName("SendNotificationToUser")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status500InternalServerError);

        // Send notification to multiple users
        group.MapPost("/send-to-users", async (
            [FromBody] SendNotificationToUsersRequest request,
            [FromServices] IRealtimeHubService hubService,
            CancellationToken ct) =>
        {
            await hubService.SendNotificationToUsersAsync(
                request.AliasIds,
                request.Notification,
                ct);

            return Results.Ok(new
            {
                success = true,
                message = $"Notification sent to {request.AliasIds.Count} users"
            });
        })
        .WithName("SendNotificationToUsers")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status500InternalServerError);

        // Send generic message to user
        group.MapPost("/send-message-to-user", async (
            [FromBody] SendMessageRequest request,
            [FromServices] IRealtimeHubService hubService,
            CancellationToken ct) =>
        {
            await hubService.SendMessageToUserAsync(
                request.AliasId,
                request.Method,
                request.Message,
                ct);

            return Results.Ok(new { success = true, message = "Message sent" });
        })
        .WithName("SendMessageToUser")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status500InternalServerError);

        // Send generic message to group
        group.MapPost("/send-message-to-group", async (
            [FromBody] SendMessageToGroupRequest request,
            [FromServices] IRealtimeHubService hubService,
            CancellationToken ct) =>
        {
            await hubService.SendMessageToGroupAsync(
                request.GroupName,
                request.Method,
                request.Message,
                ct);

            return Results.Ok(new { success = true, message = "Message sent to group" });
        })
        .WithName("SendMessageToGroup")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status500InternalServerError);

        // Get connection stats
        group.MapGet("/stats", ([FromServices] IRealtimeHubService hubService) =>
        {
            return Results.Ok(new
            {
                activeConnections = hubService.GetActiveConnectionCount(),
                timestamp = DateTimeOffset.UtcNow
            });
        })
        .WithName("GetConnectionStats")
        .Produces(StatusCodes.Status200OK);

        // Check if user is connected
        group.MapGet("/is-connected/{aliasId:guid}", (
            [FromRoute] Guid aliasId,
            [FromServices] IRealtimeHubService hubService) =>
        {
            var isConnected = hubService.IsUserConnected(aliasId);
            return Results.Ok(new
            {
                aliasId,
                isConnected,
                timestamp = DateTimeOffset.UtcNow
            });
        })
        .WithName("CheckUserConnection")
        .Produces(StatusCodes.Status200OK);
    }
}

// Request models
public record SendNotificationRequest(
    Guid AliasId,
    NotificationMessage Notification
);

public record SendNotificationToUsersRequest(
    List<Guid> AliasIds,
    NotificationMessage Notification
);

public record SendMessageRequest(
    Guid AliasId,
    string Method,
    object Message
);

public record SendMessageToGroupRequest(
    string GroupName,
    string Method,
    object Message
);
