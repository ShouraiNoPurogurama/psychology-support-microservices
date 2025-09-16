using Post.Application.Aggregates.Posts.Queries.GetPostsByTag;
using BuildingBlocks.Pagination;
using Carter;
using Mapster;
using MediatR;
using Post.Application.Aggregates.Posts.Dtos;

namespace Post.API.Endpoints.Posts;

public sealed record GetPostsByTagResponse(
    PaginatedResult<PostSummaryDto> Posts
);

public class GetPostsByTagEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/v1/tags/{categoryTagId:guid}/posts", async (
                Guid categoryTagId,
                [AsParameters] PaginationRequest pagination,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new GetPostsByTagQuery(
                    categoryTagId,
                    pagination.PageIndex,
                    pagination.PageSize
                );

                var result = await sender.Send(query, ct);

                var response = result.Adapt<GetPostsByTagResponse>();

                return Results.Ok(response);
            })
            .WithTags("Posts")
            .WithName("GetPostsByTag")
            .WithSummary("Get paginated posts by category tag")
            .Produces<GetPostsByTagResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
