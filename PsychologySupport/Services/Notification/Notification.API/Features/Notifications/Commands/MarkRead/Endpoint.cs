using Carter;
using MediatR;

namespace Notification.API.Features.Notifications.Commands.MarkRead;

public sealed record MarkReadRequest(List<Guid> NotificationIds);

public class MarkReadEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/v1/notifications/mark-read", async (
            MarkReadRequest request,
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

            var command = new MarkNotificationsReadCommand(
                RecipientAliasId: aliasId,
                NotificationIds: request.NotificationIds
            );

            var result = await sender.Send(command, ct);

            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithName("MarkNotificationsRead")
        .WithTags("Notifications")
        .Produces<MarkNotificationsReadResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status400BadRequest);
    }
}
