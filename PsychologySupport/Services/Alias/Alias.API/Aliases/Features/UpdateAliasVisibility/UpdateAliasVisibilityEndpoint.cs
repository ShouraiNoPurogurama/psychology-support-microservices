using System.Security.Claims;
using Alias.API.Aliases.Models.Enums;
using Alias.API.Extensions;
using Carter;
using Mapster;
using MediatR;

namespace Alias.API.Aliases.Features.UpdateAliasVisibility;

public record UpdateAliasVisibilityRequest(AliasVisibility Visibility);

public record UpdateAliasVisibilityResponse(
    Guid AliasId,
    AliasVisibility Visibility,
    DateTimeOffset UpdatedAt);

public class UpdateAliasVisibilityEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("/v1/me/alias/visibility", async (UpdateAliasVisibilityRequest request, ClaimsPrincipal user, ISender sender) =>
            {
                var command = new UpdateAliasVisibilityCommand(request.Visibility);
                
                var result = await sender.Send(command);
                
                var response = result.Adapt<UpdateAliasVisibilityResponse>();
                
                return Results.Ok(response);
            })
            .RequireAuthorization()
            .WithName("UpdateAliasVisibility")
            .WithTags("Aliases")
            .Produces<UpdateAliasVisibilityResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithDescription("Updates the visibility setting for the authenticated user's alias. Requires authorization.")
            .WithSummary("Update alias visibility");
    }
}