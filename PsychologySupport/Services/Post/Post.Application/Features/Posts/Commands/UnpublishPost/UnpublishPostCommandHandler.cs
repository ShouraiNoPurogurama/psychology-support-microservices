using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;
using Microsoft.EntityFrameworkCore;
using Post.Application.Abstractions.Authentication;
using Post.Application.Abstractions.Integration;
using Post.Application.Data;
using Post.Domain.Aggregates.Posts.Enums;

namespace Post.Application.Features.Posts.Commands.UnpublishPost;

internal sealed class UnpublishPostCommandHandler : ICommandHandler<UnpublishPostCommand, UnpublishPostResult>
{
    private readonly IPostDbContext _context;
    private readonly ICurrentActorAccessor _currentActorAccessor;
    private readonly IOutboxWriter _outboxWriter;

    public UnpublishPostCommandHandler(
        IPostDbContext context,
        ICurrentActorAccessor currentActorAccessor,
        IOutboxWriter outboxWriter)
    {
        _context = context;
        _currentActorAccessor = currentActorAccessor;
        _outboxWriter = outboxWriter;
    }

    public async Task<UnpublishPostResult> Handle(UnpublishPostCommand request, CancellationToken cancellationToken)
    {
        var post = await _context.Posts
            .FirstOrDefaultAsync(p => p.Id == request.PostId && !p.IsDeleted, cancellationToken);

        if (post == null)
            throw new NotFoundException("Không tìm thấy bài viết.", "POST_NOT_FOUND");

        // Change visibility to Private (unpublishes the post)
        post.ChangeVisibility(PostVisibility.Private, _currentActorAccessor.GetRequiredAliasId());

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
