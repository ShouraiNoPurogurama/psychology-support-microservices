using BuildingBlocks.Pagination;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Post.Application.Features.Posts.Dtos;
using Post.Application.Features.Posts.Queries.GetPosts;

namespace Post.API.Endpoints.Posts;

public record GetPostsRequest(
    [FromQuery] Guid[] Ids,
    int PageIndex = 1,
    int PageSize = 10,
    string? Visibility = null,
    [FromQuery] Guid[]? CategoryTagIds = null,
    string? SortBy = "CreatedAt",
    bool SortDescending = true);

public sealed record GetPostsResponse(
    PaginatedResult<PostSummaryDto> Posts
);

public class GetPostsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/v1/posts", async (
                [AsParameters] GetPostsRequest request,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new GetPostsQuery(
                    request.Ids.ToList(),
                    request.PageIndex,
                    request.PageSize,
                    request.Visibility,
                    request.CategoryTagIds?.ToList(),
                    request.SortBy,
                    request.SortDescending
                );

                var result = await sender.Send(query, ct);

                var response = new GetPostsResponse(result.Posts);

                return Results.Ok(response);
            })
            .WithTags("Posts")
            .WithName("GetPosts")
            .WithSummary("Retrieves a paginated list of posts with optional filters.")
            .WithDescription("This endpoint returns a paginated list of posts. Supports filtering by visibility, category tags, and sorting. Pagination parameters are required. Results include summary information for each post. Accessible posts depend on user permissions and moderation status.")
            .Produces<GetPostsResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}
