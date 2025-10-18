using BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;
using BuildingBlocks.Utils;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Post.Application.Abstractions.Integration;
using Post.Application.Data;
using Post.Domain.Aggregates.Comments.DomainEvents;

namespace Post.Application.Features.Comments.EventHandlers;

public sealed class CommentCreatedEventHandler : INotificationHandler<CommentCreatedEvent>
{
    private readonly IOutboxWriter _outboxWriter;
    private readonly IPostDbContext _context;
    private readonly IQueryDbContext _queryContext;

    public CommentCreatedEventHandler(
        IOutboxWriter outboxWriter,
        IPostDbContext context,
        IQueryDbContext queryContext)
    {
        _outboxWriter = outboxWriter;
        _context = context;
        _queryContext = queryContext;
    }

    public async Task Handle(CommentCreatedEvent notification, CancellationToken cancellationToken)
    {
        // Get post to find post author
        var post = await _context.Posts
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == notification.PostId, cancellationToken);

        if (post == null)
            return;

        // Don't notify if user commented on their own post
        if (post.Author.AliasId == notification.AuthorAliasId)
            return;

        // Get comment author display name
        var authorAlias = await _queryContext.AliasVersionReplica
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.AliasId == notification.AuthorAliasId, cancellationToken);

        var commentSnippet = StringUtils.GetSnippet(notification.CommentContent, 25);
        Guid? parentCommentAuthorId = null;
        if (notification.ParentCommentId.HasValue)
        {
            var parentComment = await _context.Comments
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == notification.ParentCommentId.Value, cancellationToken);
            
            if (parentComment != null)
            {
                parentCommentAuthorId = parentComment.Author.AliasId;
            }
        }

        // Publish integration event for notification
        var integrationEvent = new CommentCreatedIntegrationEvent(
            CommentId: notification.CommentId,
            PostId: notification.PostId,
            PostAuthorAliasId: post.Author.AliasId,
            CommentAuthorAliasId: notification.AuthorAliasId,
            CommentAuthorLabel: authorAlias?.Label ?? "Anonymous",
            ParentCommentId: notification.ParentCommentId,
            ParentCommentAuthorAliasId: parentCommentAuthorId,
            CommentSnippet: commentSnippet,
            CreatedAt: DateTimeOffset.UtcNow
        );

        await _outboxWriter.WriteAsync(integrationEvent, cancellationToken);
    }
}
