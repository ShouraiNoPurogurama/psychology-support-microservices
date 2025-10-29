using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;
using Microsoft.EntityFrameworkCore;
using Post.Application.Abstractions.Authentication;
using Post.Application.Abstractions.Integration;
using Post.Application.Data;
using Post.Domain.Aggregates.CategoryTags.Enums;

namespace Post.Application.Features.CategoryTags.Commands.DetachCategoryTagsFromPost;

internal sealed class DetachCategoryTagsFromPostCommandHandler : ICommandHandler<DetachCategoryTagsFromPostCommand, DetachCategoryTagsFromPostResult>
{
    private readonly IPostDbContext _context;
    private readonly ICurrentActorAccessor _currentActorAccessor;
    private readonly IOutboxWriter _outboxWriter;

    public DetachCategoryTagsFromPostCommandHandler(
        IPostDbContext context,
        ICurrentActorAccessor currentActorAccessor,
        IOutboxWriter outboxWriter)
    {
        _context = context;
        _currentActorAccessor = currentActorAccessor;
        _outboxWriter = outboxWriter;
    }

    public async Task<DetachCategoryTagsFromPostResult> Handle(DetachCategoryTagsFromPostCommand request, CancellationToken cancellationToken)
    {
        // Load the post aggregate
        var post = await _context.Posts
            .Include(p => p.Categories)
            .FirstOrDefaultAsync(p => p.Id == request.PostId && !p.IsDeleted, cancellationToken);

        if (post == null)
            throw new NotFoundException("Không tìm thấy bài viết.", "POST_NOT_FOUND");

        // Remove category tags from post via domain methods
        var detachedCategoryTagIds = new List<Guid>();
        foreach (var categoryTagId in request.CategoryTagIds)
        {
            try
            {
                post.RemoveCategory(categoryTagId, _currentActorAccessor.GetRequiredAliasId());
                detachedCategoryTagIds.Add(categoryTagId);
            }
            catch (Domain.Exceptions.PostAuthorMismatchException)
            {
                throw new ForbiddenException("Bạn không có quyền chỉnh sửa danh mục của bài viết này.", "UNAUTHORIZED_CATEGORY_TAG_OPERATION");
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
                _currentActorAccessor.GetRequiredAliasId(),
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
