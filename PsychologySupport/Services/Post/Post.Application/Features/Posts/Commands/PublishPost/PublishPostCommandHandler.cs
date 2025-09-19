using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;
using Microsoft.EntityFrameworkCore;
using Post.Application.Abstractions.Authentication;
using Post.Application.Abstractions.Integration;
using Post.Application.Data;
using Post.Domain.Aggregates.Posts.Enums;

namespace Post.Application.Features.Posts.Commands.PublishPost;

public sealed class PublishPostCommandHandler : ICommandHandler<PublishPostCommand, PublishPostResult>
{
    private readonly IPostDbContext _context;
    private readonly ICurrentActorAccessor _currentActorAccessor;
    private readonly IOutboxWriter _outboxWriter;

    public PublishPostCommandHandler(
        IPostDbContext context,
        ICurrentActorAccessor currentActorAccessor,
        IOutboxWriter outboxWriter)
    {
        _context = context;
        _currentActorAccessor = currentActorAccessor;
        _outboxWriter = outboxWriter;
    }

    public async Task<PublishPostResult> Handle(PublishPostCommand request, CancellationToken cancellationToken)
    {
        var post = await _context.Posts
            .FirstOrDefaultAsync(p => p.Id == request.PostId && !p.IsDeleted, cancellationToken);

        if (post == null)
            throw new NotFoundException("Post not found or has been deleted.", "POST_NOT_FOUND");

        // Change visibility to Public (publishes the post)
        post.ChangeVisibility(PostVisibility.Public, _currentActorAccessor.GetRequiredAliasId());

        await _context.SaveChangesAsync(cancellationToken);

        // Publish integration event via Outbox
        // await _outboxWriter.WriteAsync(
        //     new PostPublishedIntegrationEvent(
        //         post.Id,
        //         post.Author.AliasId,
        //         post.Content.Value,
        //         post.Content.Title,
        //         DateTimeOffset.UtcNow
        //     ),
        //     cancellationToken);

        return new PublishPostResult(
            post.Id,
            post.Visibility,
            DateTimeOffset.UtcNow
        );
    }
}
