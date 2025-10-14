using BuildingBlocks.Pagination;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Post.Application.Features.Posts.Dtos;
using Post.Application.Features.Reactions.Queries.GetReactedPosts;

namespace Post.API.Endpoints.Reactions;

public record GetReactedPostsRequest(
    [FromQuery] int PageIndex = 1,
    [FromQuery] int PageSize = 20,
    [FromQuery] string? ReactionCode = null,
    [FromQuery] bool SortDescending = true);

public sealed record GetReactedPostsResponse(
    PaginatedResult<PostSummaryDto> Posts
);

public class GetReactedPostsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/v1/users/{aliasId:guid}/reacted-posts", async (
                Guid aliasId,
                [AsParameters] GetReactedPostsRequest request,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new GetReactedPostsQuery(
                    aliasId,
                    request.PageIndex,
                    request.PageSize,
                    request.ReactionCode,
                    request.SortDescending
                );

                var result = await sender.Send(query, ct);

                var response = new GetReactedPostsResponse(result.Posts);

                return Results.Ok(response);
            })
            .WithTags("Reactions")
            .WithName("GetReactedPosts")
            .WithSummary("Retrieves a paginated list of posts that a user has reacted to.")
            .WithDescription("This endpoint returns a paginated list of posts that the specified user has reacted to. Supports filtering by reaction code (e.g., 'like', 'love') and sorting by reaction date. Pagination parameters are optional with defaults. Results include post summary information and user's reaction status. Accessible to all authenticated users.")
            .Produces<GetReactedPostsResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
