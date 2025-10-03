using BuildingBlocks.Pagination;
using Carter;
using MediatR;
using Post.Application.Features.Comments.Dtos;
using Post.Application.Features.Comments.Queries.GetComments;

namespace Post.API.Endpoints.Comments;

public sealed record GetCommentsRequest(
    int PageIndex = 1,
    int PageSize = 20,
    Guid? ParentCommentId = null,
    string SortBy = "CreatedAt",
    bool SortDescending = false
);

public sealed record GetCommentsResponse(
    PaginatedResult<CommentSummaryDto> Comments
);

public class GetCommentsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/v1/comments/post/{postId:guid}", async (
                Guid postId,
                [AsParameters] GetCommentsRequest request,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new GetCommentsQuery(
                    postId,
                    request.PageIndex,
                    request.PageSize,
                    request.ParentCommentId,
                    request.SortBy,
                    request.SortDescending
                );

                var result = await sender.Send(query, ct);

                var response = new GetCommentsResponse(result.Comments);

                return Results.Ok(response);
            })
            .AllowAnonymous()
            .WithTags("Comments")
            .WithName("GetComments")
            .WithSummary("Retrieves paginated comments for a post, including threaded replies.")
            .WithDescription("This endpoint returns a paginated list of comments for the specified post. Supports filtering by parentCommentId for threaded replies, sorting, and pagination. Results include comment details and reply structure. Accessible to all users.")
            .Produces<GetCommentsResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
