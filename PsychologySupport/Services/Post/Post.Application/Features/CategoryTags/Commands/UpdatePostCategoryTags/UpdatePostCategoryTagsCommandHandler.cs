using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;
using Microsoft.EntityFrameworkCore;
using Post.Application.Abstractions.Authentication;
using Post.Application.Abstractions.Integration;
using Post.Application.Data;
using Post.Domain.Aggregates.CategoryTags.Enums;

namespace Post.Application.Features.CategoryTags.Commands.UpdatePostCategoryTags;

internal sealed class UpdatePostCategoryTagsCommandHandler : ICommandHandler<UpdatePostCategoryTagsCommand, UpdatePostCategoryTagsResult>
{
    private readonly IPostDbContext _context;
    private readonly ICurrentActorAccessor _currentActorAccessor;
    private readonly IOutboxWriter _outboxWriter;

    public UpdatePostCategoryTagsCommandHandler(
        IPostDbContext context,
        ICurrentActorAccessor currentActorAccessor,
        IOutboxWriter outboxWriter)
    {
        _context = context;
        _currentActorAccessor = currentActorAccessor;
        _outboxWriter = outboxWriter;
    }

    public async Task<UpdatePostCategoryTagsResult> Handle(UpdatePostCategoryTagsCommand request, CancellationToken cancellationToken)
    {
        // Load the post aggregate
        var post = await _context.Posts
            .Include(p => p.Categories)
            .FirstOrDefaultAsync(p => p.Id == request.PostId && !p.IsDeleted, cancellationToken);

        if (post == null)
            throw new NotFoundException("Không tìm thấy bài viết.", "POST_NOT_FOUND");

        // Verify new category tags exist
        var existingCategoryTagIds = await _context.CategoryTags
            .Where(ct => request.CategoryTagIds.Contains(ct.Id))
            .Select(ct => ct.Id)
            .ToListAsync(cancellationToken);

        var missingCategoryTagIds = request.CategoryTagIds.Except(existingCategoryTagIds).ToList();
        if (missingCategoryTagIds.Any())
            throw new BadRequestException($"Category tags not found: {string.Join(", ", missingCategoryTagIds)}", "CATEGORY_TAGS_NOT_FOUND");

        // Get current category tag IDs
        var currentCategoryTagIds = post.Categories.Select(c => c.CategoryTagId).ToHashSet();
        var newCategoryTagIds = request.CategoryTagIds.ToHashSet();

        // Determine what to add and remove
        var toAdd = newCategoryTagIds.Except(currentCategoryTagIds).ToList();
        var toRemove = currentCategoryTagIds.Except(newCategoryTagIds).ToList();

        var addedCategoryTagIds = new List<Guid>();
        var removedCategoryTagIds = new List<Guid>();

        // Remove tags that are no longer needed
        foreach (var categoryTagId in toRemove)
        {
            try
            {
                post.RemoveCategory(categoryTagId, _currentActorAccessor.GetRequiredAliasId());
                removedCategoryTagIds.Add(categoryTagId);
            }
            catch (Domain.Exceptions.PostAuthorMismatchException)
            {
                throw new ForbiddenException("Bạn không có quyền chỉnh sửa danh mục của bài viết này.", "UNAUTHORIZED_CATEGORY_TAG_OPERATION");
            }
        }

        // Add new tags
        foreach (var categoryTagId in toAdd)
        {
            try
            {
                post.AddCategoryTag(categoryTagId);
                addedCategoryTagIds.Add(categoryTagId);
            }
            catch (Domain.Exceptions.InvalidPostDataException ex)
            {
                throw new BadRequestException(ex.Message, "INVALID_CATEGORY_TAG_OPERATION");
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Publish integration event via Outbox for strong consistency
        if (addedCategoryTagIds.Any() || removedCategoryTagIds.Any())
        {
            await _outboxWriter.WriteAsync(new PostCategoryTagsUpdatedIntegrationEvent(
                post.Id,
                _currentActorAccessor.GetRequiredAliasId(),
                addedCategoryTagIds.Concat(removedCategoryTagIds),
                CategoryTagUpdateStatus.Updated.ToString(),
                DateTimeOffset.UtcNow
            ), cancellationToken);
        }

        return new UpdatePostCategoryTagsResult(
            post.Id,
            addedCategoryTagIds,
            removedCategoryTagIds,
            DateTimeOffset.UtcNow
        );
    }
}
