using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;
using Post.Application.Abstractions.Authentication;
using Post.Application.Data;
using Post.Application.Integration;
using Post.Domain.Aggregates.Posts.Enums;
using Microsoft.EntityFrameworkCore;

namespace Post.Application.Aggregates.Posts.Commands.AttachMediaToPost;

internal sealed class AttachMediaToPostCommandHandler : ICommandHandler<AttachMediaToPostCommand, AttachMediaToPostResult>
{
    private readonly IPostDbContext _context;
    private readonly IActorResolver _actorResolver;
    private readonly IOutboxWriter _outboxWriter;

    public AttachMediaToPostCommandHandler(
        IPostDbContext context,
        IActorResolver actorResolver,
        IOutboxWriter outboxWriter)
    {
        _context = context;
        _actorResolver = actorResolver;
        _outboxWriter = outboxWriter;
    }

    public async Task<AttachMediaToPostResult> Handle(AttachMediaToPostCommand request, CancellationToken cancellationToken)
    {
        // Load the post aggregate
        var post = await _context.Posts
            .Include(p => p.Media)
            .FirstOrDefaultAsync(p => p.Id == request.PostId && !p.IsDeleted, cancellationToken);

        if (post == null)
            throw new NotFoundException("Post not found or has been deleted.", "POST_NOT_FOUND");

        // Authorization: Only post author can attach media
        if (post.Author.AliasId != _actorResolver.AliasId)
        {
            throw new ForbiddenException("Only the post author can attach media.", "UNAUTHORIZED_MEDIA_OPERATION");
        }

        // Add media to post via domain methods
        var attachedMediaIds = new List<Guid>();
        foreach (var mediaId in request.MediaIds)
        {
            try
            {
                post.AddMedia(mediaId);
                attachedMediaIds.Add(mediaId);
            }
            catch (Domain.Exceptions.InvalidPostDataException ex)
            {
                // Handle domain exceptions (e.g., duplicate media, max limit exceeded)
                throw new BadRequestException(ex.Message, "INVALID_MEDIA_OPERATION");
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Publish integration event via Outbox for strong consistency
        await _outboxWriter.WriteAsync(new PostMediaUpdatedIntegrationEvent(
            post.Id,
            _actorResolver.AliasId,
            attachedMediaIds,
            "ATTACHED",
            DateTimeOffset.UtcNow
        ), cancellationToken);

        return new AttachMediaToPostResult(
            post.Id,
            attachedMediaIds,
            DateTimeOffset.UtcNow
        );
    }
}
