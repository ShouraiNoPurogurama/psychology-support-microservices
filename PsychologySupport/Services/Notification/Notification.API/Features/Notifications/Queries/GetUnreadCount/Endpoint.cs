using Carter;
using MediatR;

namespace Notification.API.Features.Notifications.Queries.GetUnreadCount;

public class GetUnreadCountEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/v1/notifications/unread-count", async (
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

            var query = new GetUnreadCountQuery(aliasId);
            var result = await sender.Send(query, ct);

            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithName("GetUnreadCount")
        .WithTags("Notifications")
        .Produces<GetUnreadCountResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}
