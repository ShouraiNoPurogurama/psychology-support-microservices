using MediatR;
using Post.Application.Abstractions.Integration;
using Post.Application.Abstractions.Integration.Events;
using Post.Domain.Aggregates.Posts.DomainEvents;

namespace Post.Application.Features.Posts.EventHandlers;

public sealed class PostSharedDomainEventHandler : INotificationHandler<PostSharedEvent>
{
    private readonly IOutboxWriter _outboxWriter;

    public PostSharedDomainEventHandler(IOutboxWriter outboxWriter)
    {
        _outboxWriter = outboxWriter;
    }

    public async Task Handle(PostSharedEvent notification, CancellationToken cancellationToken)
    {
        // Increment SharesCount for post author
        var counters = new Dictionary<string, int>
        {
            { "ReactionGivenCount", 0 },
            { "ReactionReceivedCount", 0 },
            { "CommentsCount", 0 },
            { "SharesCount", 1 }
        };
        await _outboxWriter.WriteAsync(
            new AliasCountersChangedIntegrationEvent(notification.PostAuthorAliasId, counters),
            cancellationToken);
    }
}
