using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Pagination;
using Microsoft.EntityFrameworkCore;
using Post.Application.Data;
using Post.Application.Features.Comments.Queries.GetCommentsByPost;

namespace Post.Application.Features.Comments.Queries.GetCommentReplies;

internal sealed class GetCommentRepliesQueryHandler : IQueryHandler<GetCommentRepliesQuery, GetCommentRepliesResult>
{
    private readonly IPostDbContext _context;

    public GetCommentRepliesQueryHandler(IPostDbContext context)
    {
        _context = context;
    }

    public async Task<GetCommentRepliesResult> Handle(GetCommentRepliesQuery request, CancellationToken cancellationToken)
    {
        // Verify parent comment exists
        var parentCommentExists = await _context.Comments
            .AsNoTracking()
            .AnyAsync(c => c.Id == request.ParentCommentId && !c.IsDeleted, cancellationToken);

        if (!parentCommentExists)
            throw new NotFoundException("Parent comment not found or has been deleted.", "PARENT_COMMENT_NOT_FOUND");

        // Query replies with pagination and AsNoTracking
        var query = _context.Comments
            .AsNoTracking()
            .Where(c => c.Hierarchy.ParentCommentId == request.ParentCommentId && !c.IsDeleted)
            .Select(c => new CommentReplyDto(
                c.Id,
                c.PostId,
                c.Content.Value,
                c.Author.AliasId,
                c.CreatedAt,
                c.EditedAt,
                c.ReactionCount,
                c.ReplyCount,
                c.IsDeleted,
                c.Hierarchy.ParentCommentId
            ))
            .OrderBy(c => c.CreatedAt);

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply Skip/Take for pagination
        var repliesData = await query
            .Skip((request.Page - 1) * request.Size)
            .Take(request.Size)
            .ToListAsync(cancellationToken);

        // Create complete PaginatedResult object
        var paginatedResult = new PaginatedResult<CommentReplyDto>(
            request.Page,
            request.Size,
            totalCount,
            repliesData
        );

        return new GetCommentRepliesResult(paginatedResult);
    }
}
