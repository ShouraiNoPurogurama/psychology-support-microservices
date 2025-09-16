using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;
using Post.Application.Abstractions.Authentication;
using Post.Application.Data;
using Post.Application.Integration;
using Post.Domain.Aggregates.Posts.Enums;
using Microsoft.EntityFrameworkCore;

namespace Post.Application.Aggregates.Posts.Commands.RemoveMediaFromPost;

internal sealed class RemoveMediaFromPostCommandHandler : ICommandHandler<RemoveMediaFromPostCommand, RemoveMediaFromPostResult>
{
    private readonly IPostDbContext _context;
    private readonly IActorResolver _actorResolver;
    private readonly IOutboxWriter _outboxWriter;

    public RemoveMediaFromPostCommandHandler(
        IPostDbContext context,
        IActorResolver actorResolver,
        IOutboxWriter outboxWriter)
    {
        _context = context;
        _actorResolver = actorResolver;
        _outboxWriter = outboxWriter;
    }

    public async Task<RemoveMediaFromPostResult> Handle(RemoveMediaFromPostCommand request, CancellationToken cancellationToken)
    {
        // Load the post aggregate
        var post = await _context.Posts
            .Include(p => p.Media)
            .FirstOrDefaultAsync(p => p.Id == request.PostId && !p.IsDeleted, cancellationToken);

        if (post == null)
            throw new NotFoundException("Post not found or has been deleted.", "POST_NOT_FOUND");

        // Remove media from post via domain methods
        try
        {
            post.RemoveMedia(request.MediaId, _actorResolver.AliasId);
        }
        catch (Domain.Exceptions.PostAuthorMismatchException)
        {
            throw new ForbiddenException("Only the post author can remove media.", "UNAUTHORIZED_MEDIA_OPERATION");
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Publish integration event via Outbox for strong consistency
        await _outboxWriter.WriteAsync(new PostMediaUpdatedIntegrationEvent(
            post.Id,
            _actorResolver.AliasId,
            new[] { request.MediaId },
            PostMediaUpdateStatus.Removed.ToString(),
            DateTimeOffset.UtcNow
        ), cancellationToken);

        return new RemoveMediaFromPostResult(
            post.Id,
            request.MediaId,
            DateTimeOffset.UtcNow
        );
    }
}
