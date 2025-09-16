using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;
using Post.Application.Abstractions.Authentication;
using Post.Application.Data;
using Post.Application.Integration;
using Post.Domain.Aggregates.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Post.Application.Aggregates.Posts.Commands.ReportPost;

internal sealed class ReportPostCommandHandler : ICommandHandler<ReportPostCommand, ReportPostResult>
{
    private readonly IPostDbContext _context;
    private readonly IActorResolver _actorResolver;
    private readonly IOutboxWriter _outboxWriter;

    public ReportPostCommandHandler(
        IPostDbContext context,
        IActorResolver actorResolver,
        IOutboxWriter outboxWriter)
    {
        _context = context;
        _actorResolver = actorResolver;
        _outboxWriter = outboxWriter;
    }

    public async Task<ReportPostResult> Handle(ReportPostCommand request, CancellationToken cancellationToken)
    {
        // Verify post exists and is not deleted
        var post = await _context.Posts
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.PostId && !p.IsDeleted, cancellationToken);

        if (post == null)
            throw new NotFoundException("Post not found or has been deleted.", "POST_NOT_FOUND");

        // Generate report ID
        var reportId = Guid.NewGuid();
        var reportedAt = DateTimeOffset.UtcNow;

        // Emit ContentReportedIntegrationEvent to Moderation service via Outbox
        await _outboxWriter.WriteAsync(new ContentReportedIntegrationEvent(
            reportId,
            ReportedContentType.Post.ToString(),
            request.PostId,
            post.Author.AliasId,
            _actorResolver.AliasId,
            request.Reason,
            reportedAt
        ), cancellationToken);

        return new ReportPostResult(
            reportId,
            request.PostId,
            request.Reason,
            reportedAt
        );
    }
}
