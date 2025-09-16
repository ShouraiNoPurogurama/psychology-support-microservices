using Post.Application.Aggregates.Posts.Queries.GetPostsByAliasIds;
using BuildingBlocks.Pagination;
using Carter;
using Mapster;
using MediatR;

namespace Post.API.Endpoints.Posts;

public sealed record GetPostsByAliasIdsRequest(
    IEnumerable<Guid> AliasIds
);

public sealed record GetPostsByAliasIdsResponse(
    PaginatedResult<PostSummaryDto> Posts
);

public class GetPostsByAliasIdsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/v1/posts/by-alias-ids", async (
                GetPostsByAliasIdsRequest request,
                [AsParameters] PaginationRequest pagination,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new GetPostsByAliasIdsQuery(
                    request.AliasIds,
                    pagination.PageIndex,
                    pagination.PageSize
                );

                var result = await sender.Send(query, ct);

                var response = result.Adapt<GetPostsByAliasIdsResponse>();

                return Results.Ok(response);
            })
            .WithTags("Posts")
            .WithName("GetPostsByAliasIds")
            .WithSummary("Get paginated posts by multiple alias IDs")
            .Produces<GetPostsByAliasIdsResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem();
    }
}
