using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;
using Microsoft.EntityFrameworkCore;
using Post.Application.Abstractions.Authentication;
using Post.Application.Abstractions.Integration;
using Post.Application.Data;
using Post.Domain.Aggregates.Posts.Enums;

namespace Post.Application.Features.Posts.Commands.ReorderPostMedia;

internal sealed class ReorderPostMediaCommandHandler : ICommandHandler<ReorderPostMediaCommand, ReorderPostMediaResult>
{
    private readonly IPostDbContext _context;
    private readonly ICurrentActorAccessor _currentActorAccessor;
    private readonly IOutboxWriter _outboxWriter;

    public ReorderPostMediaCommandHandler(
        IPostDbContext context,
        ICurrentActorAccessor currentActorAccessor,
        IOutboxWriter outboxWriter)
    {
        _context = context;
        _currentActorAccessor = currentActorAccessor;
        _outboxWriter = outboxWriter;
    }

    public async Task<ReorderPostMediaResult> Handle(ReorderPostMediaCommand request, CancellationToken cancellationToken)
    {
        var post = await _context.Posts
            .Include(p => p.Media)
            .FirstOrDefaultAsync(p => p.Id == request.PostId && !p.IsDeleted, cancellationToken);

        if (post == null)
            throw new NotFoundException("Không tìm thấy bài viết.", "POST_NOT_FOUND");

        // Use domain method to reorder media
        post.ReorderMedia(request.OrderedMediaIds, _currentActorAccessor.GetRequiredAliasId());

        await _context.SaveChangesAsync(cancellationToken);

        // Publish integration event via Outbox
        await _outboxWriter.WriteAsync(
            new PostMediaUpdatedIntegrationEvent(
                post.Id,
                post.Author.AliasId,
                nameof(PostMediaUpdateStatus.Reordered),
                DateTimeOffset.UtcNow
            ),
            cancellationToken);

        return new ReorderPostMediaResult(
            post.Id,
            request.OrderedMediaIds,
            DateTimeOffset.UtcNow
        );
    }
}
