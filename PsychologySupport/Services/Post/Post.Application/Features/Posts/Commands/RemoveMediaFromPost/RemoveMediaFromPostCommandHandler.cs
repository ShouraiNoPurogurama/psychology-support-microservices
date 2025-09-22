using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;
using Microsoft.EntityFrameworkCore;
using Post.Application.Abstractions.Authentication;
using Post.Application.Abstractions.Integration;
using Post.Application.Data;
using Post.Domain.Aggregates.Posts.Enums;

namespace Post.Application.Features.Posts.Commands.RemoveMediaFromPost;

internal sealed class RemoveMediaFromPostCommandHandler : ICommandHandler<RemoveMediaFromPostCommand, RemoveMediaFromPostResult>
{
    private readonly IPostDbContext _context;
    private readonly ICurrentActorAccessor _currentActorAccessor;
    private readonly IOutboxWriter _outboxWriter;

    public RemoveMediaFromPostCommandHandler(
        IPostDbContext context,
        ICurrentActorAccessor currentActorAccessor,
        IOutboxWriter outboxWriter)
    {
        _context = context;
        _currentActorAccessor = currentActorAccessor;
        _outboxWriter = outboxWriter;
    }

    public async Task<RemoveMediaFromPostResult> Handle(RemoveMediaFromPostCommand request, CancellationToken cancellationToken)
    {
        var post = await _context.Posts
            .Include(p => p.Media)
            .FirstOrDefaultAsync(p => p.Id == request.PostId && !p.IsDeleted, cancellationToken);

        if (post == null)
            throw new NotFoundException("Post not found or has been deleted.", "POST_NOT_FOUND");

        // Use domain method to remove media
        post.RemoveMedia(request.MediaId, _currentActorAccessor.GetRequiredAliasId());

        await _context.SaveChangesAsync(cancellationToken);

        // Publish integration event via Outbox
        // await _outboxWriter.WriteAsync(
        //     new PostMediaUpdatedIntegrationEvent(
        //         post.Id,
        //         post.Author.AliasId,
        //         nameof(PostMediaUpdateStatus.Removed),
        //         DateTimeOffset.UtcNow
        //     ),
        //     cancellationToken);

        return new RemoveMediaFromPostResult(
            post.Id,
            request.MediaId,
            DateTimeOffset.UtcNow
        );
    }
}
