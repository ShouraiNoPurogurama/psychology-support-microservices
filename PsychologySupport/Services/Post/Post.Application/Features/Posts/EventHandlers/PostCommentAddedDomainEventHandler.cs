using MediatR;
using Post.Application.Abstractions.Integration;
using Post.Application.Abstractions.Integration.Events;
using Post.Domain.Aggregates.Posts.DomainEvents;

namespace Post.Application.Features.Posts.EventHandlers;

public sealed class PostCommentAddedDomainEventHandler : INotificationHandler<PostCommentAddedEvent>
{
    private readonly IOutboxWriter _outboxWriter;

    public PostCommentAddedDomainEventHandler(IOutboxWriter outboxWriter)
    {
        _outboxWriter = outboxWriter;
    }

    public async Task Handle(PostCommentAddedEvent notification, CancellationToken cancellationToken)
    {
        // Increment CommentsCount for comment author
        var counters = new Dictionary<string, int>
        {
            { "ReactionGivenCount", 0 },
            { "ReactionReceivedCount", 0 },
            { "CommentsCount", 1 },
            { "SharesCount", 0 }
        };
        await _outboxWriter.WriteAsync(
            new AliasCountersChangedIntegrationEvent(notification.CommentAuthorAliasId, counters),
            cancellationToken);
    }
}
