using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Post.Application.Abstractions.Authentication;
using Post.Application.Data;
using Post.Application.Integration;
using Post.Domain.Aggregates.Comment;
using Post.Domain.Aggregates.Comment.ValueObjects;
using Post.Domain.Aggregates.Post.ValueObjects;
using Post.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Post.Application.Aggregates.Comments.Commands.CreateComment;

internal sealed class CreateCommentCommandHandler : ICommandHandler<CreateCommentCommand, CreateCommentResult>
{
    private readonly IPostDbContext _context;
    private readonly IAliasVersionResolver _aliasResolver;
    private readonly IActorResolver _actorResolver;
    private readonly IOutboxWriter _outboxWriter;

    public CreateCommentCommandHandler(
        IPostDbContext context,
        IAliasVersionResolver aliasResolver,
        IOutboxWriter outboxWriter, IActorResolver actorResolver)
    {
        _context = context;
        _aliasResolver = aliasResolver;
        _outboxWriter = outboxWriter;
        _actorResolver = actorResolver;
    }

    public async Task<CreateCommentResult> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        var aliasVersionId = await _aliasResolver.GetCurrentAliasVersionIdAsync(cancellationToken);

        // Verify post exists
        var post = await _context.Posts
            .FirstOrDefaultAsync(p => p.Id == request.PostId && !p.IsDeleted, cancellationToken);

        if (post is null)
        {
            throw new NotFoundException($"Post with ID {request.PostId} not found");
        }

        Comment? parentComment = null;

        // Handle threaded comments
        if (request.ParentCommentId.HasValue)
        {
            parentComment = await _context.Comments
                // QUAN TRỌNG: Phải Include Hierarchy để dùng trong factory
                .Include(c => c.Hierarchy)
                .FirstOrDefaultAsync(c => c.Id == request.ParentCommentId.Value && !c.IsDeleted, cancellationToken);

            if (parentComment is null)
            {
                throw new NotFoundException($"Parent comment with ID {request.ParentCommentId} not found");
            }

            if (parentComment.PostId != request.PostId)
            {
                throw new BadRequestException("Parent comment must belong to the same post");
            }
        }

        // === BỎ TẤT CẢ LOGIC CŨ Ở ĐÂY ===
        // string path = request.PostId.ToString(); // XÓA
        // int level = 0; // XÓA
        // if (request.ParentCommentId.HasValue) { ... } // XÓA TOÀN BỘ KHỐI IF NÀY

        var commentId = Guid.NewGuid();
        var content = CommentContent.Create(request.Content);
        var author = AuthorInfo.Create(_actorResolver.AliasId, aliasVersionId);

        // === LOGIC MỚI ===
        // Giao toàn bộ việc tính toán cho Domain Factory
        var hierarchy = CommentHierarchy.Create(parentComment);

        var comment = Comment.Create(
            commentId,
            request.PostId,
            content.Value,
            author.AliasVersionId,
            hierarchy
        );

        _context.Comments.Add(comment);

        // Increment comment count on post
        post.IncrementCommentCount();

        // Add domain event
        var commentCreatedEvent = new CommentCreatedEvent(
            comment.Id,
            request.PostId,
            request.ParentCommentId,
            author.AliasId
        );
        await _outboxWriter.WriteAsync(commentCreatedEvent, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return new CreateCommentResult(
            comment.Id,
            request.PostId,
            request.Content,
            request.ParentCommentId,
            hierarchy.Level, // Lấy level chính xác từ object hierarchy vừa tạo
            comment.CreatedAt.Value
        );
    }
}