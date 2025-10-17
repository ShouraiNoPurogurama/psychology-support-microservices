using MediatR;
using Post.Application.Abstractions.Integration;
using Post.Application.Abstractions.Integration.Events;
using Post.Domain.Aggregates.Posts.DomainEvents;

namespace Post.Application.Features.Posts.EventHandlers;

public sealed class PostCommentRemovedDomainEventHandler : INotificationHandler<PostCommentRemovedEvent>
{
    private readonly IOutboxWriter _outboxWriter;

    public PostCommentRemovedDomainEventHandler(IOutboxWriter outboxWriter)
    {
        _outboxWriter = outboxWriter;
    }

    public async Task Handle(PostCommentRemovedEvent notification, CancellationToken cancellationToken)
    {
        // Decrement CommentsCount for comment author
        var counters = new Dictionary<string, int>
        {
            { "ReactionGivenCount", 0 },
            { "ReactionReceivedCount", 0 },
            { "CommentsCount", -1 },
            { "SharesCount", 0 }
        };
        await _outboxWriter.WriteAsync(
            new AliasCountersChangedIntegrationEvent(notification.CommentAuthorAliasId, counters),
            cancellationToken);
    }
}
