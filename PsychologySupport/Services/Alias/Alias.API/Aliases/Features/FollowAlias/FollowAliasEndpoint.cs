using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Alias.API.Aliases.Features.FollowAlias;

public record FollowAliasRequest([FromRoute] Guid FollowedAliasId);
public record FollowAliasResponse(FollowAliasResult Result);

public class FollowAliasEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/aliases/{followedAliasId:guid}/followers", async (
                [AsParameters] FollowAliasRequest request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = request.Adapt<FollowAliasCommand>();
                var result = await sender.Send(command, ct);

                var response = new FollowAliasResponse(result);

                return Results.Created($"/api/aliases/{request.FollowedAliasId}/followers", response);
            })
            .RequireAuthorization()
            .WithName("FollowAlias")
            .WithTags("Follow Operations")
            .WithSummary("Follow an alias")
            .WithDescription("Creates a follow relationship between the current user's alias and the specified alias")
            .Produces<FollowAliasResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}