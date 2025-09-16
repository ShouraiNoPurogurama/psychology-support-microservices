using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Pagination;
using Post.Application.Data;
using Microsoft.EntityFrameworkCore;

namespace Post.Application.Aggregates.Comments.Queries.GetCommentsByPost;

internal sealed class GetCommentsByPostQueryHandler : IQueryHandler<GetCommentsByPostQuery, GetCommentsByPostResult>
{
    private readonly IPostDbContext _context;

    public GetCommentsByPostQueryHandler(IPostDbContext context)
    {
        _context = context;
    }

    public async Task<GetCommentsByPostResult> Handle(GetCommentsByPostQuery request, CancellationToken cancellationToken)
    {
        // Verify post exists
        var postExists = await _context.Posts
            .AsNoTracking()
            .AnyAsync(p => p.Id == request.PostId && !p.IsDeleted, cancellationToken);

        if (!postExists)
            throw new NotFoundException("Post not found or has been deleted.", "POST_NOT_FOUND");

        // Query comments with pagination and AsNoTracking
        var query = _context.Comments
            .AsNoTracking()
            .Where(c => c.PostId == request.PostId && !c.IsDeleted)
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
        var commentsData = await query
            .Skip((request.Page - 1) * request.Size)
            .Take(request.Size)
            .ToListAsync(cancellationToken);

        // Create complete PaginatedResult object
        var paginatedResult = new PaginatedResult<CommentReplyDto>(
            request.Page,
            request.Size,
            totalCount,
            commentsData
        );

        return new GetCommentsByPostResult(paginatedResult);
    }
}
