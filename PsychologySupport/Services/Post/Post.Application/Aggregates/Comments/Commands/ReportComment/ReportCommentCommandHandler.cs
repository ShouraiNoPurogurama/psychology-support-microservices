using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;
using Post.Application.Abstractions.Authentication;
using Post.Application.Data;
using Post.Application.Integration;
using Microsoft.EntityFrameworkCore;

namespace Post.Application.Aggregates.Comments.Commands.ReportComment;

internal sealed class ReportCommentCommandHandler : ICommandHandler<ReportCommentCommand, ReportCommentResult>
{
    private readonly IPostDbContext _context;
    private readonly IActorResolver _actorResolver;
    private readonly IOutboxWriter _outboxWriter;

    public ReportCommentCommandHandler(
        IPostDbContext context,
        IActorResolver actorResolver,
        IOutboxWriter outboxWriter)
    {
        _context = context;
        _actorResolver = actorResolver;
        _outboxWriter = outboxWriter;
    }

    public async Task<ReportCommentResult> Handle(ReportCommentCommand request, CancellationToken cancellationToken)
    {
        // Verify comment exists and is not deleted
        var comment = await _context.Comments
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.CommentId && !c.IsDeleted, cancellationToken);

        if (comment == null)
            throw new NotFoundException("Comment not found or has been deleted.", "COMMENT_NOT_FOUND");

        // Generate report ID
        var reportId = Guid.NewGuid();
        var reportedAt = DateTimeOffset.UtcNow;

        // Emit ContentReportedIntegrationEvent to Moderation service via Outbox
        await _outboxWriter.WriteAsync(new ContentReportedIntegrationEvent(
            reportId,
            "COMMENT",
            request.CommentId,
            comment.Author.AliasId,
            _actorResolver.AliasId,
            request.Reason,
            reportedAt
        ), cancellationToken);

        return new ReportCommentResult(
            reportId,
            request.CommentId,
            request.Reason,
            reportedAt
        );
    }
}
