using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;
using Microsoft.EntityFrameworkCore;
using Post.Application.Abstractions.Authentication;
using Post.Application.Abstractions.Integration;
using Post.Application.Data;
using Post.Domain.Aggregates.Posts.Enums;

namespace Post.Application.Features.Posts.Commands.AttachMediaToPost;

public sealed class AttachMediaToPostCommandHandler : ICommandHandler<AttachMediaToPostCommand, AttachMediaToPostResult>
{
    private readonly IPostDbContext _context;
    private readonly ICurrentActorAccessor _currentActorAccessor;
    private readonly IOutboxWriter _outboxWriter;

    public AttachMediaToPostCommandHandler(
        IPostDbContext context,
        ICurrentActorAccessor currentActorAccessor,
        IOutboxWriter outboxWriter)
    {
        _context = context;
        _currentActorAccessor = currentActorAccessor;
        _outboxWriter = outboxWriter;
    }

    public async Task<AttachMediaToPostResult> Handle(AttachMediaToPostCommand request, CancellationToken cancellationToken)
    {
        var post = await _context.Posts
            .Include(p => p.Media)
            .FirstOrDefaultAsync(p => p.Id == request.PostId && !p.IsDeleted, cancellationToken);

        if (post == null)
            throw new NotFoundException("Post not found or has been deleted.", "POST_NOT_FOUND");

        post.AddMedia(request.MediaId, request.Position);

        await _context.SaveChangesAsync(cancellationToken);
        //
        // await _outboxWriter.WriteAsync(
        //     new PostMediaUpdatedIntegrationEvent(
        //         post.Id,
        //         post.Author.AliasId,
        //         nameof(PostMediaUpdateStatus.Attached),
        //         DateTimeOffset.UtcNow
        //     ),
        //     cancellationToken);

        return new AttachMediaToPostResult(
            post.Id,
            request.MediaId,
            DateTimeOffset.UtcNow
        );
    }
}
