using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Microsoft.EntityFrameworkCore;
using Post.Application.Abstractions.Authentication;
using Post.Application.Abstractions.Integration;
using Post.Application.Data;
using Post.Domain.Aggregates.Comments;
using Post.Domain.Aggregates.Comments.DomainEvents;
using Post.Domain.Aggregates.Comments.ValueObjects;
using Post.Domain.Aggregates.Posts.ValueObjects;

namespace Post.Application.Features.Comments.Commands.CreateComment;

internal sealed class CreateCommentCommandHandler : ICommandHandler<CreateCommentCommand, CreateCommentResult>
{
    private readonly IPostDbContext _context;
    private readonly IAliasVersionAccessor _aliasAccessor;
    private readonly ICurrentActorAccessor _currentActorAccessor;
    private readonly IOutboxWriter _outboxWriter;

    public CreateCommentCommandHandler(
        IPostDbContext context,
        IAliasVersionAccessor aliasAccessor,
        IOutboxWriter outboxWriter, ICurrentActorAccessor currentActorAccessor)
    {
        _context = context;
        _aliasAccessor = aliasAccessor;
        _outboxWriter = outboxWriter;
        _currentActorAccessor = currentActorAccessor;
    }

    public async Task<CreateCommentResult> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        var aliasVersionId = await _aliasAccessor.GetRequiredCurrentAliasVersionIdAsync(cancellationToken);

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

        var content = CommentContent.Create(request.Content);
        var author = AuthorInfo.Create(_currentActorAccessor.GetRequiredAliasId(), aliasVersionId);
        
        var comment = Comment.Create(
            post.Id,
            author.AliasId,
            content.Value,
            author.AliasVersionId,
            parentComment
        );
        
        comment.Approve("bypass-moderation", Guid.Empty);
        
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
            comment.Hierarchy.Level, 
            comment.CreatedAt
        );
    }
}