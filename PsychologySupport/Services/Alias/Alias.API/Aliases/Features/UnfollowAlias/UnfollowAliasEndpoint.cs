using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Alias.API.Aliases.Features.UnfollowAlias;


public record UnfollowAliasRequest([FromRoute] Guid FollowedAliasId);
public record UnfollowAliasResponse(UnfollowAliasResult Result);

public class UnfollowAliasEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/aliases/{followedAliasId:guid}/followers", async (
                [AsParameters] UnfollowAliasRequest request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = request.Adapt<UnfollowAliasCommand>();
                var result = await sender.Send(command, ct);
                var response = new UnfollowAliasResponse(result);
                
                return Results.Ok(response);
            })
            .RequireAuthorization()
            .WithName("UnfollowAlias")
            .WithSummary("Unfollow an alias")
            .WithDescription("Removes the follow relationship between the current user's alias and the specified alias")
            .Produces<UnfollowAliasResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}