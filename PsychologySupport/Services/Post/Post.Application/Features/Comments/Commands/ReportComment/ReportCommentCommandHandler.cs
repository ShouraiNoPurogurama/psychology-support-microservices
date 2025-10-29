using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;
using Microsoft.EntityFrameworkCore;
using Post.Application.Abstractions.Authentication;
using Post.Application.Abstractions.Integration;
using Post.Application.Data;
using Post.Domain.Aggregates.Shared.Enums;

namespace Post.Application.Features.Comments.Commands.ReportComment;

internal sealed class ReportCommentCommandHandler : ICommandHandler<ReportCommentCommand, ReportCommentResult>
{
    private readonly IPostDbContext _context;
    private readonly ICurrentActorAccessor _currentActorAccessor;
    private readonly IOutboxWriter _outboxWriter;

    public ReportCommentCommandHandler(
        IPostDbContext context,
        ICurrentActorAccessor currentActorAccessor,
        IOutboxWriter outboxWriter)
    {
        _context = context;
        _currentActorAccessor = currentActorAccessor;
        _outboxWriter = outboxWriter;
    }

    public async Task<ReportCommentResult> Handle(ReportCommentCommand request, CancellationToken cancellationToken)
    {
        // Verify comment exists and is not deleted
        var comment = await _context.Comments
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.CommentId && !c.IsDeleted, cancellationToken);

        if (comment == null)
            throw new NotFoundException("Không tìm thấy bình luận.", "COMMENT_NOT_FOUND");

        // Generate report ID
        var reportId = Guid.NewGuid();
        var reportedAt = DateTimeOffset.UtcNow;

        // Emit ContentReportedIntegrationEvent to Moderation service via Outbox
        await _outboxWriter.WriteAsync(new ContentReportedIntegrationEvent(
            reportId,
            ReportedContentType.Comment.ToString(),
            request.CommentId,
            comment.Author.AliasId,
            _currentActorAccessor.GetRequiredAliasId(),
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
