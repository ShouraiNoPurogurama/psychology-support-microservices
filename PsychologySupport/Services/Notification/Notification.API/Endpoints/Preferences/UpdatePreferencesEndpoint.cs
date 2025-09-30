using Carter;
using MediatR;
using Notification.API.Features.Preferences.Commands.UpsertPreferences;

namespace Notification.API.Endpoints.Preferences;

public sealed record UpdatePreferencesRequest(
    bool ReactionsEnabled,
    bool CommentsEnabled,
    bool MentionsEnabled,
    bool FollowsEnabled,
    bool ModerationEnabled,
    bool BotEnabled,
    bool SystemEnabled
);

public class UpdatePreferencesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/v1/notifications/preferences", async (
            UpdatePreferencesRequest request,
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

            var command = new UpsertPreferencesCommand(
                AliasId: aliasId,
                ReactionsEnabled: request.ReactionsEnabled,
                CommentsEnabled: request.CommentsEnabled,
                MentionsEnabled: request.MentionsEnabled,
                FollowsEnabled: request.FollowsEnabled,
                ModerationEnabled: request.ModerationEnabled,
                BotEnabled: request.BotEnabled,
                SystemEnabled: request.SystemEnabled
            );

            var result = await sender.Send(command, ct);

            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithName("UpdateNotificationPreferences")
        .WithTags("Preferences")
        .Produces<UpsertPreferencesResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}
