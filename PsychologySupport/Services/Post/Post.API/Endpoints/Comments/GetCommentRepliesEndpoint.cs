using BuildingBlocks.Pagination;
using Carter;
using MediatR;
using Post.Application.Features.Comments.Dtos;
using Post.Application.Features.Comments.Queries.GetCommentReplies;

namespace Post.API.Endpoints.Comments;

public sealed record GetCommentRepliesRequest(
    int PageIndex = 1,
    int PageSize = 20,
    string SortBy = "CreatedAt",
    bool SortDescending = false
);

public sealed record GetCommentRepliesResponse(
    PaginatedResult<ReplySummaryDto> Comments
);

public class GetCommentRepliesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/v1/comments/{parentCommentId:guid}/replies", async (
                Guid parentCommentId,
                [AsParameters] GetCommentRepliesRequest request,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new GetCommentRepliesQuery(
                    parentCommentId,
                    request.PageIndex,
                    request.PageSize
                );

                var result = await sender.Send(query, ct);

                var response = new GetCommentRepliesResponse(result.Replies);

                return Results.Ok(response);
            })
            .AllowAnonymous()
            .WithTags("Comments")
            .WithName("GetCommentReplies")
            .WithSummary("Retrieves paginated replies for a specific comment.")
            .WithDescription(
                "This endpoint returns a paginated list of replies for the specified parent comment. Supports pagination parameters. Results include reply details. Accessible to all users.")
            .Produces<GetCommentRepliesResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}