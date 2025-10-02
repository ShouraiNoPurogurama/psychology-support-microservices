using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Notification.API.Features.Notifications.Commands.MarkAllRead;

public class MarkAllReadEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/v1/notifications/mark-all-read", async (
            [FromQuery] DateTimeOffset? cutoffTime,
            HttpContext httpContext,
            ISender sender,
            CancellationToken ct) =>
        {
            // Get aliasId from authenticated user
            var aliasIdClaim = httpContext.User.FindFirst("aliasId")?.Value;
            if (string.IsNullOrEmpty(aliasIdClaim) || !Guid.TryParse(aliasIdClaim, out var aliasId))
            {
                return Results.Unauthorized();
            }

            var command = new MarkAllNotificationsReadCommand(
                RecipientAliasId: aliasId,
                CutoffTime: cutoffTime
            );

            var result = await sender.Send(command, ct);

            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithName("MarkAllNotificationsRead")
        .WithTags("Notifications")
        .Produces<MarkAllNotificationsReadResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}
