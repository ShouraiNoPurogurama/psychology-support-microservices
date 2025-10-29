using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;
using Microsoft.EntityFrameworkCore;
using Post.Application.Abstractions.Authentication;
using Post.Application.Abstractions.Integration;
using Post.Application.Data;

namespace Post.Application.Features.Posts.Commands.ToggleCommentsLock;

internal sealed class ToggleCommentsLockCommandHandler : ICommandHandler<ToggleCommentsLockCommand, ToggleCommentsLockResult>
{
    private readonly IPostDbContext _context;
    private readonly ICurrentActorAccessor _currentActorAccessor;
    private readonly IOutboxWriter _outboxWriter;

    public ToggleCommentsLockCommandHandler(
        IPostDbContext context,
        ICurrentActorAccessor currentActorAccessor,
        IOutboxWriter outboxWriter)
    {
        _context = context;
        _currentActorAccessor = currentActorAccessor;
        _outboxWriter = outboxWriter;
    }

    public async Task<ToggleCommentsLockResult> Handle(ToggleCommentsLockCommand request, CancellationToken cancellationToken)
    {
        var post = await _context.Posts
            .FirstOrDefaultAsync(p => p.Id == request.PostId && !p.IsDeleted, cancellationToken);

        if (post == null)
            throw new NotFoundException("Không tìm thấy bài viết.", "POST_NOT_FOUND");

        // Set specific lock state instead of toggling
        var wasLocked = post.IsCommentsLocked;
        if (wasLocked != request.IsLocked)
        {
            post.ToggleCommentsLock(_currentActorAccessor.GetRequiredAliasId());
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
