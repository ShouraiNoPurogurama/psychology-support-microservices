using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;
using Microsoft.EntityFrameworkCore;
using Post.Application.Abstractions.Integration;
using Post.Application.Data;

namespace Post.Application.Features.Posts.Commands.SoftDeletePost;

internal sealed class SoftDeletePostCommandHandler : ICommandHandler<SoftDeletePostCommand, SoftDeletePostResult>
{
    private readonly IPostDbContext _context;
    private readonly IOutboxWriter _outboxWriter;

    public SoftDeletePostCommandHandler(IPostDbContext context, IOutboxWriter outboxWriter)
    {
        _context = context;
        _outboxWriter = outboxWriter;
    }

    public async Task<SoftDeletePostResult> Handle(SoftDeletePostCommand request, CancellationToken cancellationToken)
    {
        var post = await _context.Posts
            .FirstOrDefaultAsync(p => p.Id == request.PostId && !p.IsDeleted, cancellationToken);

        if (post is null)
        {
            throw new NotFoundException($"Post with ID {request.PostId} not found");
        }

        // The domain aggregate will validate delete permissions
        post.SoftDelete(request.DeleterAliasId);

        // Publish integration event for downstream services (Feed, Search, etc.)
        await _outboxWriter.WriteAsync(
            new PostDeletedIntegrationEvent(post.Id, request.DeleterAliasId, post.DeletedAt!.Value),
            cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return new SoftDeletePostResult(
            post.Id,
            post.DeletedAt!.Value
        );
    }
}
