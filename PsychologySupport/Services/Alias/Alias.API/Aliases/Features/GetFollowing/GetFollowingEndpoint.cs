using BuildingBlocks.Pagination;
using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Alias.API.Aliases.Features.GetFollowing;

public record GetFollowingRequest(
    [FromRoute] Guid AliasId,
    [FromQuery] int PageIndex = 1,
    [FromQuery] int PageSize = 20
);
public record GetFollowingResponse(PaginatedResult<FollowingDto> Following);

public class GetFollowingEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/aliases/{aliasId:guid}/following", async (
                [AsParameters] GetFollowingRequest request,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = request.Adapt<GetFollowingQuery>();
                var result = await sender.Send(query, ct);
                var response = new GetFollowingResponse(result.Following);
                
                return Results.Ok(response);
            })
            .RequireAuthorization()
            .WithName("GetFollowing")
            .WithTags("Follow Operations")
            .WithSummary("Get aliases that an alias is following")
            .WithDescription("Retrieves a paginated list of aliases that the specified alias is following")
            .Produces<GetFollowingResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}