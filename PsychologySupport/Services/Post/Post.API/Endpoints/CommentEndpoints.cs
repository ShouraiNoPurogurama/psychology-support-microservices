using BuildingBlocks.Pagination;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Post.Application.Aggregates.Comments.Commands.CreateComment;
using Post.Application.Aggregates.Comments.Queries.GetComments;

namespace Post.API.Endpoints;

public class CommentEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/comments")
            .WithTags("Comments")
            .RequireAuthorization();

        // Create Comment
        group.MapPost("/", CreateComment)
            .WithName("CreateComment")
            .WithSummary("Create a new comment on a post")
            .Produces<CreateCommentResult>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        // Get Comments for a Post (with threaded replies)
        group.MapGet("/post/{postId:guid}", GetComments)
            .WithName("GetComments")
            .WithSummary("Get comments for a post with threaded replies")
            .Produces<PaginatedResult<CommentDto>>(StatusCodes.Status200OK)
            .AllowAnonymous();

        // Create Reply to Comment
        group.MapPost("/{commentId:guid}/replies", CreateReply)
            .WithName("CreateReply")
            .WithSummary("Create a reply to an existing comment")
            .Produces<CreateCommentResult>(StatusCodes.Status201Created)
            .ProducesValidationProblem();
    }

    private static async Task<IResult> CreateComment(
        CreateCommentRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new CreateCommentCommand(
            request.PostId,
            request.Content,
            request.ParentCommentId
        );

        var result = await mediator.Send(command, cancellationToken);
        return Results.Created($"/api/comments/{result.CommentId}", result);
    }

    private static async Task<IResult> GetComments(
        Guid postId,
        [AsParameters] GetCommentsRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetCommentsQuery(
            postId,
            request.Page,
            request.PageSize,
            request.ParentCommentId,
            request.SortBy,
            request.SortDescending
        );

        var result = await mediator.Send(query, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> CreateReply(
        Guid commentId,
        CreateReplyRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new CreateCommentCommand(
            request.PostId,
            request.Content,
            commentId // Parent comment ID
        );

        var result = await mediator.Send(command, cancellationToken);
        return Results.Created($"/api/comments/{result.CommentId}", result);
    }
}

// Request DTOs for endpoint parameters
public record CreateCommentRequest(
    Guid PostId,
    string Content,
    Guid? ParentCommentId = null
);

public record GetCommentsRequest(
    int Page = 1,
    int PageSize = 20,
    Guid? ParentCommentId = null,
    string SortBy = "CreatedAt",
    bool SortDescending = false
);

public record CreateReplyRequest(
    Guid PostId,
    string Content
);
