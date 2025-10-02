using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Notification.API.Features.Notifications.Models;

namespace Notification.API.Features.Notifications.Queries.GetNotifications;

public class GetNotificationsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/v1/notifications", async (
            [FromQuery] int? limit,
            [FromQuery] string? cursor,
            [FromQuery] int? type,
            [FromQuery] bool? unreadOnly,
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

            var query = new GetNotificationsQuery(
                RecipientAliasId: aliasId,
                Limit: limit ?? 20,
                Cursor: cursor,
                Type: type.HasValue ? (NotificationType)type.Value : null,
                UnreadOnly: unreadOnly ?? false
            );

            var result = await sender.Send(query, ct);

            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithName("GetNotifications")
        .WithTags("Notifications")
        .Produces<GetNotificationsResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}
