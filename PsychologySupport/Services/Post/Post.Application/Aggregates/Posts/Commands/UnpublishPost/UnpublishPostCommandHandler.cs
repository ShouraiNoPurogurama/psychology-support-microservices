using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;
using Post.Application.Abstractions.Authentication;
using Post.Application.Data;
using Post.Application.Integration;
using Microsoft.EntityFrameworkCore;
using Post.Domain.Aggregates.Posts.Enums;

namespace Post.Application.Aggregates.Posts.Commands.UnpublishPost;

internal sealed class UnpublishPostCommandHandler : ICommandHandler<UnpublishPostCommand, UnpublishPostResult>
{
    private readonly IPostDbContext _context;
    private readonly IActorResolver _actorResolver;
    private readonly IOutboxWriter _outboxWriter;

    public UnpublishPostCommandHandler(
        IPostDbContext context,
        IActorResolver actorResolver,
        IOutboxWriter outboxWriter)
    {
        _context = context;
        _actorResolver = actorResolver;
        _outboxWriter = outboxWriter;
    }

    public async Task<UnpublishPostResult> Handle(UnpublishPostCommand request, CancellationToken cancellationToken)
    {
        var post = await _context.Posts
            .FirstOrDefaultAsync(p => p.Id == request.PostId && !p.IsDeleted, cancellationToken);

        if (post == null)
            throw new NotFoundException("Post not found or has been deleted.", "POST_NOT_FOUND");

        // Change visibility to Private (unpublishes the post)
        post.ChangeVisibility(PostVisibility.Private, _actorResolver.AliasId);

        await _context.SaveChangesAsync(cancellationToken);

        // Publish integration event via Outbox
        await _outboxWriter.WriteAsync(
            new PostUnpublishedIntegrationEvent(
                post.Id,
                post.Author.AliasId,
                DateTimeOffset.UtcNow
            ),
            cancellationToken);

        return new UnpublishPostResult(
            post.Id,
            post.Visibility.ToString(),
            DateTimeOffset.UtcNow
        );
    }
}
