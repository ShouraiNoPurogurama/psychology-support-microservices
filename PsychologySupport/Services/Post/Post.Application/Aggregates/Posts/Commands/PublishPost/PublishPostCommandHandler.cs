using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;
using Post.Application.Abstractions.Authentication;
using Post.Application.Data;
using Post.Application.Integration;
using Microsoft.EntityFrameworkCore;
using Post.Domain.Aggregates.Posts.Enums;

namespace Post.Application.Aggregates.Posts.Commands.PublishPost;

internal sealed class PublishPostCommandHandler : ICommandHandler<PublishPostCommand, PublishPostResult>
{
    private readonly IPostDbContext _context;
    private readonly IActorResolver _actorResolver;
    private readonly IOutboxWriter _outboxWriter;

    public PublishPostCommandHandler(
        IPostDbContext context,
        IActorResolver actorResolver,
        IOutboxWriter outboxWriter)
    {
        _context = context;
        _actorResolver = actorResolver;
        _outboxWriter = outboxWriter;
    }

    public async Task<PublishPostResult> Handle(PublishPostCommand request, CancellationToken cancellationToken)
    {
        var post = await _context.Posts
            .FirstOrDefaultAsync(p => p.Id == request.PostId && !p.IsDeleted, cancellationToken);

        if (post == null)
            throw new NotFoundException("Post not found or has been deleted.", "POST_NOT_FOUND");

        // Change visibility to Public (publishes the post)
        post.ChangeVisibility(PostVisibility.Public, _actorResolver.AliasId);

        await _context.SaveChangesAsync(cancellationToken);

        // Publish integration event via Outbox
        await _outboxWriter.WriteAsync(
            new PostPublishedIntegrationEvent(
                post.Id,
                post.Author.AliasId,
                post.Content.Value,
                post.Content.Title,
                DateTimeOffset.UtcNow
            ),
            cancellationToken);

        return new PublishPostResult(
            post.Id,
            post.Visibility.ToString(),
            DateTimeOffset.UtcNow
        );
    }
}
