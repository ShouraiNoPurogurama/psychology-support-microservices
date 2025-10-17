using System.Security.Claims;
using Alias.API.Aliases.Dtos;
using Alias.API.Aliases.Models.Aliases.Enums;
using Carter;
using Mapster;
using MediatR;

namespace Alias.API.Aliases.Features.UpdatePreferences;

public record UpdatePreferencesRequest(
    PreferenceTheme? Theme = null,
    PreferenceLanguage? Language = null,
    bool? NotificationsEnabled = null);

public record UpdatePreferencesResponse(
    UserPreferencesDto Preferences,
    DateTimeOffset UpdatedAt);

public class UpdatePreferencesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/v1/me/alias/preferences", async (UpdatePreferencesRequest request, ClaimsPrincipal user, ISender sender) =>
            {
                var command = new UpdatePreferencesCommand(
                    request.Theme,
                    request.Language,
                    request.NotificationsEnabled);
                
                var result = await sender.Send(command);
                
                var response = result.Adapt<UpdatePreferencesResponse>();
                
                return Results.Ok(response);
            })
            .RequireAuthorization()
            .WithName("UpdateMyPreferences")
            .WithTags("Preferences")
            .Produces<UpdatePreferencesResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithDescription("Updates the user preferences for the authenticated user's alias. All fields are optional - only provided fields will be updated. Requires authorization.")
            .WithSummary("Update my preferences");
    }
}
