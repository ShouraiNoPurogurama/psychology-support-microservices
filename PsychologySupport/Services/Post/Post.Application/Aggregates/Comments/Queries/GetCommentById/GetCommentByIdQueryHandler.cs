using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Post.Application.Data;
using Microsoft.EntityFrameworkCore;
using Post.Application.Aggregates.Comments.Dtos;
using Post.Application.Aggregates.Comments.Queries.GetComments;
using Post.Application.Aggregates.Posts.Dtos;

namespace Post.Application.Aggregates.Comments.Queries.GetCommentById;

internal sealed class GetCommentByIdQueryHandler : IQueryHandler<GetCommentByIdQuery, GetCommentByIdResult>
{
    private readonly IPostDbContext _context;
    private readonly IQueryDbContext _queryContext;

    public GetCommentByIdQueryHandler(IPostDbContext context, IQueryDbContext queryContext)
    {
        _context = context;
        _queryContext = queryContext;
    }

    public async Task<GetCommentByIdResult> Handle(GetCommentByIdQuery request, CancellationToken cancellationToken)
    {
        // Get comment with AsNoTracking
        var comment = await _context.Comments
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.CommentId, cancellationToken);

        if (comment == null)
            return new GetCommentByIdResult(null);

        // Get author display name from query context (no EF join)
        var authorAlias = await _queryContext.AliasVersionReplica
                              .AsNoTracking()
                              .FirstOrDefaultAsync(a => a.AliasId == comment.Author.AliasId, cancellationToken)
                          ?? throw new NotFoundException("Không tìm thấy tác giả bình luận.", "COMMENT_AUTHOR_NOT_FOUND");

        var authorDto = new AuthorDto(authorAlias.AliasId, authorAlias.Label, authorAlias.AvatarUrl);

        var commentDto = new CommentDto(
            comment.Id,
            comment.PostId,
            comment.Content.Value,
            authorDto,
            new HierarchyDto(
                comment.Hierarchy.ParentCommentId,
                comment.Hierarchy.Path,
                comment.Hierarchy.Level
            ),
            comment.CreatedAt,
            comment.EditedAt,
            comment.ReactionCount,
            comment.ReplyCount,
            comment.IsDeleted
        );


        return new GetCommentByIdResult(commentDto);
    }
}