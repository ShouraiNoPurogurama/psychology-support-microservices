using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;
using Microsoft.EntityFrameworkCore;
using Post.Application.Abstractions.Integration;
using Post.Application.Data;
using Post.Domain.Aggregates.Posts.Enums;

namespace Post.Application.Features.Posts.Commands.AbandonPost;

/// <summary>
/// Handler for marking abandoned posts and triggering Emo Bot via integration event.
/// </summary>
internal sealed class AbandonPostCommandHandler : ICommandHandler<AbandonPostCommand, AbandonPostResult>
{
    private readonly IPostDbContext _context;
    private readonly IOutboxWriter _outboxWriter;

    public AbandonPostCommandHandler(IPostDbContext context, IOutboxWriter outboxWriter)
    {
        _context = context;
        _outboxWriter = outboxWriter;
    }

    public async Task<AbandonPostResult> Handle(AbandonPostCommand request, CancellationToken cancellationToken)
    {
        var post = await _context.Posts
            .FirstOrDefaultAsync(p => p.Id == request.PostId && !p.IsDeleted, cancellationToken);

        if (post is null)
        {
            throw new NotFoundException($"Post with ID {request.PostId} not found");
        }

        // Check if post is in Creating state (abandoned if left in this state too long)
        if (post.Status != PostStatus.Creating)
        {
            throw new InvalidOperationException($"Post {request.PostId} is not in Creating state. Cannot mark as abandoned.");
        }

        // Check if abandonment event was already emitted to avoid duplicates
        if (post.IsAbandonmentEventEmitted)
        {
            throw new InvalidOperationException($"Post {request.PostId} already has an abandonment event emitted.");
        }

        // Mark the post as having emitted the abandonment event
        post.MarkAbandonmentEventEmitted();

        var abandonedAt = DateTimeOffset.UtcNow;

        // Publish integration event to trigger Emo Bot in Moderation service
        await _outboxWriter.WriteAsync(
            new PostAbandonedIntegrationEvent(
                post.Id,
                post.Content.Value,
                post.Author.AliasId,
                abandonedAt),
            cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return new AbandonPostResult(
            post.Id,
            post.Status.ToString(),
            abandonedAt
        );
    }
}
