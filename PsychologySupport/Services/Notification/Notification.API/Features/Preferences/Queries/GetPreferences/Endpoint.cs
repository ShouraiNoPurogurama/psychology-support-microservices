using Carter;
using MediatR;

namespace Notification.API.Features.Preferences.Queries.GetPreferences;

public class GetPreferencesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/v1/notifications/preferences", async (
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

            var query = new GetPreferencesQuery(aliasId);
            var result = await sender.Send(query, ct);

            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithName("GetNotificationPreferences")
        .WithTags("Preferences")
        .Produces<GetPreferencesResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}
