using BuildingBlocks.Pagination;
using Carter;
using Mapster;
using MediatR;
using Post.Application.Features.Posts.Dtos;
using Post.Application.Features.Posts.Queries.GetPostsByUser;

namespace Post.API.Endpoints.Posts;

public sealed record GetPostsByUserResponse(
    PaginatedResult<PostDto> Posts
);

public class GetPostsByUserEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/v1/users/{authorAliasId:guid}/posts", async (
                Guid authorAliasId,
                [AsParameters] PaginationRequest pagination,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new GetPostsByUserQuery(
                    authorAliasId,
                    pagination.PageIndex,
                    pagination.PageSize
                );

                var result = await sender.Send(query, ct);

                var response = new GetPostsByUserResponse(result.Posts);

                return Results.Ok(response);
            })
            .WithTags("Posts")
            .WithName("GetPostsByUser")
            .WithSummary("Get paginated posts by a specific user")
            .Produces<GetPostsByUserResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
