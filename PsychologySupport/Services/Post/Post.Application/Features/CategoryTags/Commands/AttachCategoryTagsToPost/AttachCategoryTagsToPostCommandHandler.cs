using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;
using Microsoft.EntityFrameworkCore;
using Post.Application.Abstractions.Authentication;
using Post.Application.Abstractions.Integration;
using Post.Application.Data;
using Post.Domain.Aggregates.CategoryTags.Enums;

namespace Post.Application.Features.CategoryTags.Commands.AttachCategoryTagsToPost;

internal sealed class AttachCategoryTagsToPostCommandHandler : ICommandHandler<AttachCategoryTagsToPostCommand, AttachCategoryTagsToPostResult>
{
    private readonly IPostDbContext _context;
    private readonly ICurrentActorAccessor _currentActorAccessor;
    private readonly IOutboxWriter _outboxWriter;

    public AttachCategoryTagsToPostCommandHandler(
        IPostDbContext context,
        ICurrentActorAccessor currentActorAccessor,
        IOutboxWriter outboxWriter)
    {
        _context = context;
        _currentActorAccessor = currentActorAccessor;
        _outboxWriter = outboxWriter;
    }

    public async Task<AttachCategoryTagsToPostResult> Handle(AttachCategoryTagsToPostCommand request, CancellationToken cancellationToken)
    {
        // Load the post aggregate
        var post = await _context.Posts
            .Include(p => p.Categories)
            .FirstOrDefaultAsync(p => p.Id == request.PostId && !p.IsDeleted, cancellationToken);

        if (post == null)
            throw new NotFoundException("Không tìm thấy bài viết.", "POST_NOT_FOUND");

        // Verify category tags exist
        var existingCategoryTagIds = await _context.CategoryTags
            .Where(ct => request.CategoryTagIds.Contains(ct.Id))
            .Select(ct => ct.Id)
            .ToListAsync(cancellationToken);

        var missingCategoryTagIds = request.CategoryTagIds.Except(existingCategoryTagIds).ToList();
        if (missingCategoryTagIds.Any())
            throw new BadRequestException($"Category tags not found: {string.Join(", ", missingCategoryTagIds)}", "CATEGORY_TAGS_NOT_FOUND");

        // Add category tags to post via domain methods
        var attachedCategoryTagIds = new List<Guid>();
        foreach (var categoryTagId in request.CategoryTagIds)
        {
            try
            {
                post.AddCategoryTag(categoryTagId);
                attachedCategoryTagIds.Add(categoryTagId);
            }
            catch (Domain.Exceptions.InvalidPostDataException ex)
            {
                // Handle domain exceptions (e.g., duplicate tags, max limit exceeded)
                throw new BadRequestException(ex.Message, "INVALID_CATEGORY_TAG_OPERATION");
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Publish integration event via Outbox for strong consistency
        await _outboxWriter.WriteAsync(new PostCategoryTagsUpdatedIntegrationEvent(
            post.Id,
            _currentActorAccessor.GetRequiredAliasId(),
            attachedCategoryTagIds,
            nameof(CategoryTagUpdateStatus.Attached),
            DateTimeOffset.UtcNow
        ), cancellationToken);

        return new AttachCategoryTagsToPostResult(
            post.Id,
            attachedCategoryTagIds,
            DateTimeOffset.UtcNow
        );
    }
}
