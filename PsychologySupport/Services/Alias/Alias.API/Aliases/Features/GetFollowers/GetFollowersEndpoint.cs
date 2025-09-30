using BuildingBlocks.Pagination;
using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Alias.API.Aliases.Features.GetFollowers;

public record GetFollowersRequest(
    [FromRoute] Guid AliasId,
    [FromQuery] int PageIndex = 1,
    [FromQuery] int PageSize = 20
);
public record GetFollowersResponse(PaginatedResult<FollowerDto> Followers);

public class GetFollowersEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/v1/aliases/{aliasId:guid}/followers", async (
                [AsParameters] GetFollowersRequest request,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = request.Adapt<GetFollowersQuery>();
                var result = await sender.Send(query, ct);
                var response = new GetFollowersResponse(result.Followers);
                
                return Results.Ok(response);
            })
            .RequireAuthorization()
            .WithName("GetFollowers")
            .WithTags("Follow Operations")
            .WithSummary("Get followers of an alias")
            .WithDescription("Retrieves a paginated list of aliases that are following the specified alias")
            .Produces<GetFollowersResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}