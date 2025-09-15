using System.Security.Claims;
using Alias.API.Extensions;
using Carter;
using Mapster;
using MediatR;

namespace Alias.API.Domains.PublicProfiles.Features.GetPublicProfile;


public record GetPublicProfileResponse();

public class GetPublicProfileEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/v1/me/alias", async (ISender sender, ClaimsPrincipal user) =>
        {
            var aliasId = user.GetAliasId();
            var query = new GetPublicProfileQuery(aliasId);
            var result = await sender.Send(query);
            var response = result.Adapt<GetPublicProfileResponse>();
            return Results.Ok(response);
        })
        .RequireAuthorization()
        .WithName("GetPublicProfile")
        .WithTags("PublicProfiles")
        .Produces<GetPublicProfileResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithDescription("Retrieves the public profile for the authenticated user's alias. Requires authorization. Returns public profile details on success.")
        .WithSummary("Get public profile by alias");
    }
}