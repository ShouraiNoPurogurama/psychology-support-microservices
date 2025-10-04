using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Utils;
using Microsoft.EntityFrameworkCore;
using Post.Application.Abstractions.Authentication;
using Post.Application.Abstractions.Integration;
using Post.Application.Data;
using Post.Domain.Aggregates.Posts.ValueObjects;
using Post.Domain.Aggregates.Reaction;
using Post.Domain.Aggregates.Reaction.DomainEvents;
using Post.Domain.Aggregates.Reaction.ValueObjects;
using Post.Domain.Aggregates.Reactions.Enums;
using StackExchange.Redis;

namespace Post.Application.Features.Reactions.Commands.CreateReaction;

public class CreateReactionCommandHandler : ICommandHandler<CreateReactionCommand, CreateReactionResult>
{
    private readonly IPostDbContext _context;
    private readonly IAliasVersionAccessor _aliasAccessor;
    private readonly ICurrentActorAccessor _currentActorAccessor;
    private readonly IOutboxWriter _outboxWriter;
    private readonly IDatabase _redisDatabase;

    public CreateReactionCommandHandler(
        IPostDbContext context,
        IAliasVersionAccessor aliasAccessor,
        IOutboxWriter outboxWriter,
        ICurrentActorAccessor currentActorAccessor,
        IConnectionMultiplexer redisConnection)
    {
        _context = context;
        _aliasAccessor = aliasAccessor;
        _outboxWriter = outboxWriter;
        _currentActorAccessor = currentActorAccessor;
        _redisDatabase = redisConnection.GetDatabase(); // Lấy IDatabase từ connection
    }

    public async Task<CreateReactionResult> Handle(CreateReactionCommand request, CancellationToken cancellationToken)
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
            // 2. Logic nghiệp vụ cốt lõi (giữ nguyên)
            var aliasVersionId = await _aliasAccessor.GetRequiredCurrentAliasVersionIdAsync(cancellationToken);
            await ValidateTargetExists(request.TargetType, request.TargetId, cancellationToken);

            var existingReaction = await _context.Reactions
                .AsNoTracking() // Tối ưu: Chỉ đọc, không cần tracking
                .Where(r => r.Target.TargetType == request.TargetType &&
                            r.Target.TargetId == request.TargetId &&
                            r.Author.AliasId == currentAliasId &&
                            !r.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);

            if (existingReaction != null)
            {
                return new CreateReactionResult(
                    existingReaction.Id,
                    request.TargetType,
                    request.TargetId,
                    (ReactionCode)Enum.Parse(typeof(ReactionCode), existingReaction.Type.Code, true),
                    existingReaction.CreatedAt
                );
            }

            var reactionType = CreateReactionType(request.ReactionCode);
            var author = AuthorInfo.Create(currentAliasId, aliasVersionId);
            var reaction = Reaction.Create(
                request.TargetType,
                request.TargetId,
                reactionType.Code,
                reactionType.Emoji,
                reactionType.Weight,
                true,
                author.AliasId,
                author.AliasVersionId
            );
            _context.Reactions.Add(reaction);

            await UpdateTargetCounters(request.TargetType, request.TargetId, 1, cancellationToken);

            var reactionAddedEvent = new ReactionAddedEvent(
                reaction.Id,
                request.TargetType.ToString().ToLower(),
                request.TargetId,
                request.ReactionCode.ToString().ToLower(),
                currentAliasId
            );
            await _outboxWriter.WriteAsync(reactionAddedEvent, cancellationToken);

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex) when (DbUtils.IsUniqueViolation(ex))
            {
                // Safeguard: Unique index constraint hit
                throw new ConflictException("Bạn đã thực hiện reaction cho nội dung này rồi.", "REACTION_ALREADY_EXISTS");
            }

            return new CreateReactionResult(
                reaction.Id,
                request.TargetType,
                request.TargetId,
                request.ReactionCode,
                reaction.CreatedAt
            );
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

    private async Task ValidateTargetExists(ReactionTargetType targetType, Guid targetId, CancellationToken cancellationToken)
    {
        bool exists = targetType switch
        {
            ReactionTargetType.Post => await _context.Posts.AnyAsync(p => p.Id == targetId && !p.IsDeleted, cancellationToken),
            ReactionTargetType.Comment => await _context.Comments.AnyAsync(c => c.Id == targetId && !c.IsDeleted, cancellationToken),
            _ => throw new BadRequestException($"Invalid target type: {targetType}")
        };
        if (!exists) throw new NotFoundException($"{targetType} with ID {targetId} not found");
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

    private static ReactionType CreateReactionType(ReactionCode code)
    {
        return code switch
        {
            ReactionCode.Like => ReactionType.Create("like", "👍", 1),
            ReactionCode.Heart => ReactionType.Create("heart", "❤️", 2),
            ReactionCode.Laugh => ReactionType.Create("laugh", "😂", 1),
            ReactionCode.Wow => ReactionType.Create("wow", "😮", 1),
            ReactionCode.Sad => ReactionType.Create("sad", "😢", 1),
            ReactionCode.Angry => ReactionType.Create("angry", "😠", 1),
            _ => throw new BadRequestException($"Invalid reaction code: {code}")
        };
    }
}