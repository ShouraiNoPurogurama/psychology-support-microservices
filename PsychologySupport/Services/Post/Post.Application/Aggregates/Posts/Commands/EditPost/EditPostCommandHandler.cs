using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Post.Application.Abstractions.Authentication;
using Post.Application.Data;
using Post.Application.Integration;
using Post.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Post.Domain.Aggregates.Posts;

namespace Post.Application.Aggregates.Posts.Commands.EditPost;

internal sealed class EditPostCommandHandler : ICommandHandler<EditPostCommand, EditPostResult>
{
    private readonly IPostDbContext _context;
    private readonly IAliasVersionResolver _aliasResolver;
    private readonly IOutboxWriter _outboxWriter;

    public EditPostCommandHandler(
        IPostDbContext context,
        IAliasVersionResolver aliasResolver,
        IOutboxWriter outboxWriter)
    {
        _context = context;
        _aliasResolver = aliasResolver;
        _outboxWriter = outboxWriter;
    }

    public async Task<EditPostResult> Handle(EditPostCommand request, CancellationToken cancellationToken)
    {
        var aliasContext = await _aliasResolver.GetCurrentAliasVersionIdAsync(cancellationToken);
        
        var post = await _context.Posts
            .FirstOrDefaultAsync(p => p.Id == request.PostId, cancellationToken);

        if (post is null)
        {
            throw new NotFoundException($"Post with ID {request.PostId} not found");
        }

        // Verify ownership
        if (post.Author.AliasId != request.AliasId)
        {
            throw new UnauthorizedAccessException("You can only edit your own posts");
        }

        // Update content
        post.UpdateContent(request.Content, request.Title, request.AliasId);

        // Update media if provided
        if (request.MediaUrls?.Any() == true)
        {
            // Remove existing media
            var existingMedia = await _context.PostMedia
                .Where(pm => pm.PostId == request.PostId)
                .ToListAsync(cancellationToken);
            
            _context.PostMedia.RemoveRange(existingMedia);

            //Add new media
            foreach (var (mediaUrl, index) in request.MediaUrls.Select((url, i) => (url, i)))
            {
                var postMedia = PostMedia.Create(
                    request.PostId,
                    Guid.NewGuid(),
                    index,
                    null, // caption
                    null  // altText
                );
                _context.PostMedia.Add(postMedia);
            }
        }

        // Update categories if provided
        if (request.CategoryTagIds?.Any() == true)
        {
            // Remove existing categories
            var existingCategories = await _context.PostCategories
                .Where(pc => pc.PostId == request.PostId)
                .ToListAsync(cancellationToken);
            
            _context.PostCategories.RemoveRange(existingCategories);

            // Add new categories
            foreach (var categoryTagId in request.CategoryTagIds)
            {
                var postCategory = PostCategory.Create(
                    request.PostId,
                    categoryTagId
                );
                _context.PostCategories.Add(postCategory);
            }
        }

        // Add domain event
        var postUpdatedEvent = new PostUpdatedEvent(post.Id, request.AliasId);
        await _outboxWriter.WriteAsync(postUpdatedEvent, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return new EditPostResult(post.Id, post.LastModified.Value);
    }
}
