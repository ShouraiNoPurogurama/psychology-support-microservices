using Post.Application.Aggregates.Posts.Queries.GetPostById;
using Post.Application.Aggregates.Posts.Dtos;
using Carter;
using Mapster;
using MediatR;

namespace Post.API.Endpoints.Posts;

public sealed record GetPostByIdResponse(
    Guid Id,
    string Content,
    string? Title,
    AuthorDto Author,
    string Visibility,
    string ModerationStatus,
    int ReactionCount,
    int CommentCount,
    int ViewCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset? EditedAt,
    DateTimeOffset PublishedAt,
    IReadOnlyList<string> MediaUrls,
    IReadOnlyList<string> Categories
);

public class GetPostByIdEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/v1/posts/{postId:guid}", async (
                Guid postId,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new GetPostByIdQuery(postId);

                var result = await sender.Send(query, ct);

                var response = result.Adapt<GetPostByIdResponse>();

                return Results.Ok(response);
            })
            .WithTags("Posts")
            .WithName("GetPostById")
            .WithSummary("Retrieves a post by its unique identifier.")
            .WithDescription("This endpoint returns the details of a post specified by postId. The response includes author, content, media, categories, and engagement metrics. The post must exist and be accessible to the requesting user based on visibility and moderation status.")
            .Produces<GetPostByIdResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
