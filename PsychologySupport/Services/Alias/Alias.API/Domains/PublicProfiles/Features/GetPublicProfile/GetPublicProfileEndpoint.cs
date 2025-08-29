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
        });
    }
}