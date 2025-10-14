using MediatR;
using Post.Application.Abstractions.Integration;
using Post.Application.Abstractions.Integration.Events;
using Post.Domain.Aggregates.Posts.DomainEvents;

namespace Post.Application.Features.Posts.EventHandlers;

public sealed class PostReactionRemovedDomainEventHandler : INotificationHandler<PostReactionRemovedEvent>
{
    private readonly IOutboxWriter _outboxWriter;

    public PostReactionRemovedDomainEventHandler(IOutboxWriter outboxWriter)
    {
        _outboxWriter = outboxWriter;
    }

    public async Task Handle(PostReactionRemovedEvent notification, CancellationToken cancellationToken)
    {
        // Decrement ReactionGivenCount for reactor
        var reactorCounters = new Dictionary<string, int>
        {
            { "ReactionGivenCount", -1 },
            { "ReactionReceivedCount", 0 },
            { "CommentsCount", 0 },
            { "SharesCount", 0 }
        };
        await _outboxWriter.WriteAsync(
            new AliasCountersChangedIntegrationEvent(notification.ReactorAliasId, reactorCounters),
            cancellationToken);

        // Decrement ReactionReceivedCount for post author (if different from reactor)
        if (notification.ReactorAliasId != notification.PostAuthorAliasId)
        {
            var authorCounters = new Dictionary<string, int>
            {
                { "ReactionGivenCount", 0 },
                { "ReactionReceivedCount", -1 },
                { "CommentsCount", 0 },
                { "SharesCount", 0 }
            };
            await _outboxWriter.WriteAsync(
                new AliasCountersChangedIntegrationEvent(notification.PostAuthorAliasId, authorCounters),
                cancellationToken);
        }
    }
}
