using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;
using Post.Application.Abstractions.Authentication;
using Post.Application.Data;
using Post.Application.Integration;
using Post.Domain.Aggregates.CategoryTags.Enums;
using Microsoft.EntityFrameworkCore;

namespace Post.Application.Aggregates.CategoryTags.Commands.DetachCategoryTagsFromPost;

internal sealed class DetachCategoryTagsFromPostCommandHandler : ICommandHandler<DetachCategoryTagsFromPostCommand, DetachCategoryTagsFromPostResult>
{
    private readonly IPostDbContext _context;
    private readonly IActorResolver _actorResolver;
    private readonly IOutboxWriter _outboxWriter;

    public DetachCategoryTagsFromPostCommandHandler(
        IPostDbContext context,
        IActorResolver actorResolver,
        IOutboxWriter outboxWriter)
    {
        _context = context;
        _actorResolver = actorResolver;
        _outboxWriter = outboxWriter;
    }

    public async Task<DetachCategoryTagsFromPostResult> Handle(DetachCategoryTagsFromPostCommand request, CancellationToken cancellationToken)
    {
        // Load the post aggregate
        var post = await _context.Posts
            .Include(p => p.Categories)
            .FirstOrDefaultAsync(p => p.Id == request.PostId && !p.IsDeleted, cancellationToken);

        if (post == null)
            throw new NotFoundException("Post not found or has been deleted.", "POST_NOT_FOUND");

        // Remove category tags from post via domain methods
        var detachedCategoryTagIds = new List<Guid>();
        foreach (var categoryTagId in request.CategoryTagIds)
        {
            try
            {
                post.RemoveCategory(categoryTagId, _actorResolver.AliasId);
                detachedCategoryTagIds.Add(categoryTagId);
            }
            catch (Domain.Exceptions.PostAuthorMismatchException)
            {
                throw new ForbiddenException("Only the post author can modify category tags.", "UNAUTHORIZED_CATEGORY_TAG_OPERATION");
            }
            catch (Domain.Exceptions.InvalidPostDataException ex)
            {
                // Handle domain exceptions - category tag not found on post is not an error
                // We only add to detached list if it was actually present
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Publish integration event via Outbox for strong consistency
        if (detachedCategoryTagIds.Any())
        {
            await _outboxWriter.WriteAsync(new PostCategoryTagsUpdatedIntegrationEvent(
                post.Id,
                _actorResolver.AliasId,
                detachedCategoryTagIds,
                CategoryTagUpdateStatus.Detached.ToString(),
                DateTimeOffset.UtcNow
            ), cancellationToken);
        }

        return new DetachCategoryTagsFromPostResult(
            post.Id,
            detachedCategoryTagIds,
            DateTimeOffset.UtcNow
        );
    }
}
