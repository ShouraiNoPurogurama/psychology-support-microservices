using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;
using Microsoft.EntityFrameworkCore;
using Post.Application.Abstractions.Authentication;
using Post.Application.Abstractions.Integration;
using Post.Application.Data;
using Post.Domain.Aggregates.Posts.Enums;

namespace Post.Application.Features.Posts.Commands.RejectPost;

internal sealed class RejectPostCommandHandler : ICommandHandler<RejectPostCommand, RejectPostResult>
{
    private readonly IPostDbContext _context;
    private readonly ICurrentActorAccessor _currentActorAccessor;
    private readonly IOutboxWriter _outboxWriter;

    public RejectPostCommandHandler(
        IPostDbContext context,
        ICurrentActorAccessor currentActorAccessor,
        IOutboxWriter outboxWriter)
    {
        _context = context;
        _currentActorAccessor = currentActorAccessor;
        _outboxWriter = outboxWriter;
    }

    public async Task<RejectPostResult> Handle(RejectPostCommand request, CancellationToken cancellationToken)
    {
        var post = await _context.Posts
            .FirstOrDefaultAsync(p => p.Id == request.PostId && !p.IsDeleted, cancellationToken);

        if (post == null)
            throw new NotFoundException("Post not found or has been deleted.", "POST_NOT_FOUND");

        // Use existing domain method
        var reasons = new List<string> { request.Reason };
        post.Reject(reasons, "1.0", _currentActorAccessor.GetRequiredAliasId());

        await _context.SaveChangesAsync(cancellationToken);

        // Publish integration event via Outbox
        await _outboxWriter.WriteAsync(
            new PostRejectedIntegrationEvent(
                post.Id,
                post.Author.AliasId,
                _currentActorAccessor.GetRequiredAliasId(),
                request.Reason,
                DateTimeOffset.UtcNow
            ),
            cancellationToken);

        return new RejectPostResult(
            post.Id,
            ModerationStatus.Rejected.ToString(),
            request.Reason,
            DateTimeOffset.UtcNow
        );
    }
}
