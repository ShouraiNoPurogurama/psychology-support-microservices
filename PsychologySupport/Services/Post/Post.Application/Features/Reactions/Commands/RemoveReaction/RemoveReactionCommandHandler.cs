using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Microsoft.EntityFrameworkCore;
using Post.Application.Abstractions.Authentication;
using Post.Application.Abstractions.Integration;
using Post.Application.Data;
using Post.Domain.Aggregates.Reaction.DomainEvents;
using Post.Domain.Aggregates.Reactions.Enums;
using StackExchange.Redis;

namespace Post.Application.Features.Reactions.Commands.RemoveReaction;

public sealed class RemoveReactionCommandHandler : ICommandHandler<RemoveReactionCommand, RemoveReactionResult>
{
    private readonly IPostDbContext _context;
    private readonly ICurrentActorAccessor _currentActorAccessor;
    private readonly IOutboxWriter _outboxWriter;
    private readonly IDatabase _redisDatabase;

    public RemoveReactionCommandHandler(
        IPostDbContext context,
        IOutboxWriter outboxWriter,
        ICurrentActorAccessor currentActorAccessor,
        IConnectionMultiplexer redisConnection) 
    {
        _context = context;
        _outboxWriter = outboxWriter;
        _currentActorAccessor = currentActorAccessor;
        _redisDatabase = redisConnection.GetDatabase(); 
    }

    public async Task<RemoveReactionResult> Handle(RemoveReactionCommand request, CancellationToken cancellationToken)
    {
        var currentAliasId = _currentActorAccessor.GetRequiredAliasId();

        // 1. Logic Distributed Lock
        var lockKey = $"lock:reaction:{currentAliasId}:{request.TargetId}";
        var lockToken = Guid.NewGuid().ToString();
        var lockExpiry = TimeSpan.FromSeconds(10);

        if (!await _redisDatabase.StringSetAsync(lockKey, lockToken, lockExpiry, When.NotExists))
        {
            throw new ConflictException("Hành động của bạn đang được xử lý, vui lòng không thao tác quá nhanh.", "CONCURRENT_REACTION_OPERATION");
        }

        try
        {
            // 2. Logic nghiệp vụ cốt lõi
            var reaction = await _context.Reactions
                .Where(r => r.Target.TargetType == request.TargetType &&
                            r.Target.TargetId == request.TargetId &&
                            r.Author.AliasId == currentAliasId &&
                            !r.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);

            if (reaction is null)
            {
                // Nếu không có reaction để xóa, trả về thành công
                return new RemoveReactionResult(true, DateTimeOffset.UtcNow);
            }

            reaction.Delete(currentAliasId);

            await UpdateTargetCounters(request.TargetType, request.TargetId, -1, cancellationToken);

            // Emit domain event for alias counters
            if (request.TargetType == ReactionTargetType.Post)
            {
                var post = await _context.Posts.FirstAsync(p => p.Id == request.TargetId, cancellationToken);
                post.RemoveReaction(currentAliasId);
            }

            var reactionRemovedEvent = new ReactionRemovedEvent(
                reaction.Id,
                request.TargetType,
                request.TargetId,
                reaction.Type.Code,
                currentAliasId
            );
            await _outboxWriter.WriteAsync(reactionRemovedEvent, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            return new RemoveReactionResult(true, reaction.DeletedAt!.Value);
        }
        finally
        {
            // 3. Giải phóng lock
            if (await _redisDatabase.StringGetAsync(lockKey) == lockToken)
            {
                await _redisDatabase.KeyDeleteAsync(lockKey);
            }
        }
    }

    private async Task UpdateTargetCounters(ReactionTargetType targetType, Guid targetId, int value, CancellationToken cancellationToken)
    {
        switch (targetType)
        {
            case ReactionTargetType.Post:
                var post = await _context.Posts.FirstAsync(p => p.Id == targetId, cancellationToken);
                if (value > 0) post.IncrementReactionCount(value); else post.DecrementReactionCount(Math.Abs(value));
                break;
            case ReactionTargetType.Comment:
                var comment = await _context.Comments.FirstAsync(c => c.Id == targetId, cancellationToken);
                if (value > 0) comment.IncrementReactionCount(value); else comment.DecrementReactionCount(Math.Abs(value));
                break;
        }
    }
}