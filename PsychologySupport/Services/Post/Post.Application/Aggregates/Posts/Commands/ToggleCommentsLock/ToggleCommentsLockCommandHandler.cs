using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;
using Post.Application.Abstractions.Authentication;
using Post.Application.Data;
using Post.Application.Integration;
using Microsoft.EntityFrameworkCore;

namespace Post.Application.Aggregates.Posts.Commands.ToggleCommentsLock;

internal sealed class ToggleCommentsLockCommandHandler : ICommandHandler<ToggleCommentsLockCommand, ToggleCommentsLockResult>
{
    private readonly IPostDbContext _context;
    private readonly IActorResolver _actorResolver;
    private readonly IOutboxWriter _outboxWriter;

    public ToggleCommentsLockCommandHandler(
        IPostDbContext context,
        IActorResolver actorResolver,
        IOutboxWriter outboxWriter)
    {
        _context = context;
        _actorResolver = actorResolver;
        _outboxWriter = outboxWriter;
    }

    public async Task<ToggleCommentsLockResult> Handle(ToggleCommentsLockCommand request, CancellationToken cancellationToken)
    {
        var post = await _context.Posts
            .FirstOrDefaultAsync(p => p.Id == request.PostId && !p.IsDeleted, cancellationToken);

        if (post == null)
            throw new NotFoundException("Post not found or has been deleted.", "POST_NOT_FOUND");

        // Set specific lock state instead of toggling
        var wasLocked = post.IsCommentsLocked;
        if (wasLocked != request.IsLocked)
        {
            post.ToggleCommentsLock(_actorResolver.AliasId);
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Publish integration event via Outbox
        await _outboxWriter.WriteAsync(
            new PostCommentsLockStatusChangedIntegrationEvent(
                post.Id,
                post.Author.AliasId,
                post.IsCommentsLocked,
                DateTimeOffset.UtcNow
            ),
            cancellationToken);

        return new ToggleCommentsLockResult(
            post.Id,
            post.IsCommentsLocked,
            DateTimeOffset.UtcNow
        );
    }
}
