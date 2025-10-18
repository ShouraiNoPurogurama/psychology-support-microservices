using BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;
using BuildingBlocks.Utils;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Post.Application.Abstractions.Integration;
using Post.Application.Data;
using Post.Domain.Aggregates.Reaction.DomainEvents;
using Post.Domain.Aggregates.Reactions.Enums;

namespace Post.Application.Features.Reactions.EventHandlers;

public sealed class ReactionCreatedEventHandler : INotificationHandler<ReactionCreatedEvent>
{
    private readonly IOutboxWriter _outboxWriter;
    private readonly IPostDbContext _context;
    private readonly IQueryDbContext _queryContext;

    public ReactionCreatedEventHandler(
        IOutboxWriter outboxWriter,
        IPostDbContext context,
        IQueryDbContext queryContext)
    {
        _outboxWriter = outboxWriter;
        _context = context;
        _queryContext = queryContext;
    }

    public async Task Handle(ReactionCreatedEvent notification, CancellationToken cancellationToken)
    {
        Guid targetAuthorAliasId;
        string commentSnippet = "";
        string targetType = notification.TargetType.ToString().ToLower();

        // Get target author based on target type
        if (notification.TargetType == ReactionTargetType.Post)
        {
            var post = await _context.Posts
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == notification.TargetId, cancellationToken);

            if (post == null)
                return;

            targetAuthorAliasId = post.Author.AliasId;
        }
        else if (notification.TargetType == ReactionTargetType.Comment)
        {
            var comment = await _context.Comments
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == notification.TargetId, cancellationToken);

            if (comment == null)
                return;

            targetAuthorAliasId = comment.Author.AliasId;
            commentSnippet = StringUtils.GetSnippet(comment.Content.Value, 25);
        }
        else
        {
            return; // Unknown target type
        }

        // Don't notify if user reacted to their own content
        if (targetAuthorAliasId == notification.ReactorAliasId)
            return;

        // Get reactor display name
        var reactorAlias = await _queryContext.AliasVersionReplica
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.AliasId == notification.ReactorAliasId, cancellationToken);

        // Publish integration event for notification
        var integrationEvent = new ReactionAddedIntegrationEvent(
            ReactionId: notification.ReactionId,
            TargetType: targetType,
            TargetId: notification.TargetId,
            TargetAuthorAliasId: targetAuthorAliasId,
            ReactorAliasId: notification.ReactorAliasId,
            ReactorLabel: reactorAlias?.Label ?? "Anonymous",
            ReactionCode: notification.ReactionCode,
            ReactedAt: DateTimeOffset.UtcNow,
            CommentSnippet: commentSnippet
        );

        await _outboxWriter.WriteAsync(integrationEvent, cancellationToken);
    }
}
