using System.Security.Claims;
using Alias.API.Aliases.Dtos;
using Alias.API.Extensions;
using Carter;
using Mapster;
using MediatR;

namespace Alias.API.Aliases.Features.GetPublicProfile;

public record GetPublicProfileRequest(Guid AliasId);

public record GetPublicProfileResponse(
    Guid AliasId,
    string Label,
    string? AvatarUrl,
    int Followers,
    int Followings,
    int Posts,
    DateTimeOffset CreatedAt
);

public class GetPublicProfileEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/v1/aliases/{aliasId:guid}/profile", async (Guid aliasId, ISender sender) =>
        {
            var query = new GetPublicProfileQuery(aliasId);
            var result = await sender.Send(query);
            var response = result.Profile.Adapt<GetPublicProfileResponse>();
            return Results.Ok(response);
        })
        .WithName("GetPublicProfile")
        .WithTags("PublicProfiles")
        .Produces<GetPublicProfileResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithDescription("Retrieves the public profile for the specified alias ID. Returns public profile details on success.")
        .WithSummary("Get public profile by alias ID");

        app.MapGet("/v1/me/alias", async (ISender sender, ClaimsPrincipal user) =>
        {
            var aliasId = user.GetAliasId();
            var query = new GetPublicProfileQuery(aliasId);
            var result = await sender.Send(query);
            var response = result.Profile.Adapt<GetPublicProfileResponse>();
            return Results.Ok(response);
        })
        .RequireAuthorization()
        .WithName("GetMyPublicProfile")
        .WithTags("PublicProfiles")
        .Produces<GetPublicProfileResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithDescription("Retrieves the public profile for the authenticated user's alias. Requires authorization.")
        .WithSummary("Get my public profile");
    }
}