using System.Security.Claims;
using Alias.API.Aliases.Dtos;
using Alias.API.Extensions;
using Carter;
using Mapster;
using MediatR;

namespace Alias.API.Aliases.Features.GetPreferences;

public record GetPreferencesResponse(
    UserPreferencesDto Preferences
);

public class GetPreferencesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/v1/me/alias/preferences", async (ISender sender, ClaimsPrincipal user) =>
        {
            var aliasId = user.GetAliasId();
            var query = new GetPreferencesQuery(aliasId);
            var result = await sender.Send(query);
            var response = result.Adapt<GetPreferencesResponse>();
            return Results.Ok(response);
        })
        .RequireAuthorization()
        .WithName("GetMyPreferences")
        .WithTags("Preferences")
        .Produces<GetPreferencesResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithDescription("Retrieves the user preferences for the authenticated user's alias. Requires authorization.")
        .WithSummary("Get my preferences");
    }
}
