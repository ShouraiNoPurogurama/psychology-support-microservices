using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Post.Application.Abstractions.Authentication;
using Post.Application.Data;
using Post.Application.Integration;
using Post.Domain.Aggregates.Reaction;
using Post.Domain.Aggregates.Reaction.ValueObjects;
using Post.Domain.Aggregates.Posts.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Post.Domain.Aggregates.Reaction.DomainEvents;
using Post.Domain.Aggregates.Reactions.Enums;

namespace Post.Application.Aggregates.Reactions.Commands.CreateReaction;

internal sealed class CreateReactionCommandHandler : ICommandHandler<CreateReactionCommand, CreateReactionResult>
{
    private readonly IPostDbContext _context;
    private readonly IAliasVersionResolver _aliasResolver;
    private readonly IActorResolver _actorResolver;
    private readonly IOutboxWriter _outboxWriter;

    public CreateReactionCommandHandler(
        IPostDbContext context,
        IAliasVersionResolver aliasResolver,
        IOutboxWriter outboxWriter, IActorResolver actorResolver)
    {
        _context = context;
        _aliasResolver = aliasResolver;
        _outboxWriter = outboxWriter;
        _actorResolver = actorResolver;
    }

    public async Task<CreateReactionResult> Handle(CreateReactionCommand request, CancellationToken cancellationToken)
    {
        var aliasVersionId = await _aliasResolver.GetCurrentAliasVersionIdAsync(cancellationToken);

        // Validate target exists
        await ValidateTargetExists(request.TargetType, request.TargetId, cancellationToken);

        // Check if user already has a reaction on this target
        var existingReaction = await _context.Reactions
            .Where(r => r.Target.TargetType == request.TargetType.ToString() &&
                        r.Target.TargetId == request.TargetId &&
                        r.Author.AliasId == _actorResolver.AliasId &&
                        !r.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingReaction != null)
        {
            // Update existing reaction if different type
            if (existingReaction.Type.Code != request.ReactionCode.ToString().ToLower())
            {
                var newReactionType = CreateReactionType(request.ReactionCode);
                existingReaction.UpdateType(newReactionType, _actorResolver.AliasId);

                var reactionUpdatedEvent = new ReactionUpdatedEvent(
                    existingReaction.Id,
                    request.TargetType.ToString().ToLower(),
                    request.TargetId,
                    request.ReactionCode.ToString().ToLower(),
                    _actorResolver.AliasId
                );
                await _outboxWriter.WriteAsync(reactionUpdatedEvent, cancellationToken);

                await _context.SaveChangesAsync(cancellationToken);

                return new CreateReactionResult(
                    existingReaction.Id,
                    request.TargetType,
                    request.TargetId,
                    request.ReactionCode,
                    existingReaction.LastModified!.Value
                );
            }

            // Same reaction type - return existing
            return new CreateReactionResult(
                existingReaction.Id,
                request.TargetType,
                request.TargetId,
                request.ReactionCode,
                existingReaction.CreatedAt
            );
        }

        // Create new reaction
        var reactionId = Guid.NewGuid();
        var target = ReactionTarget.Create(request.TargetType.ToString().ToLower(), request.TargetId);
        var reactionType = CreateReactionType(request.ReactionCode);
        var author = AuthorInfo.Create(_actorResolver.AliasId, aliasVersionId);

        var reaction = Reaction.Create(
            request.TargetType.ToString().ToLower(),
            request.TargetId,
            reactionType.Code,
            reactionType.Emoji,
            reactionType.Weight,
            true,
            author.AliasId,
            author.AliasVersionId
        );
        _context.Reactions.Add(reaction);

        // Update target counters
        await UpdateTargetCounters(request.TargetType, request.TargetId, cancellationToken);

        // Add domain event
        var reactionAddedEvent = new ReactionAddedEvent(
            reaction.Id,
            request.TargetType.ToString().ToLower(),
            request.TargetId,
            request.ReactionCode.ToString().ToLower(),
            _actorResolver.AliasId
        );
        await _outboxWriter.WriteAsync(reactionAddedEvent, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return new CreateReactionResult(
            reaction.Id,
            request.TargetType,
            request.TargetId,
            request.ReactionCode,
            reaction.CreatedAt
        );
    }

    private async Task ValidateTargetExists(ReactionTargetType targetType, Guid targetId, CancellationToken cancellationToken)
    {
        bool exists = targetType switch
        {
            ReactionTargetType.Post => await _context.Posts.AnyAsync(p => p.Id == targetId && !p.IsDeleted, cancellationToken),
            ReactionTargetType.Comment => await _context.Comments.AnyAsync(c => c.Id == targetId && !c.IsDeleted, cancellationToken),
            _ => throw new BadRequestException($"Invalid target type: {targetType}")
        };

        if (!exists)
        {
            throw new NotFoundException($"{targetType} with ID {targetId} not found");
        }
    }

    private async Task UpdateTargetCounters(ReactionTargetType targetType, Guid targetId, CancellationToken cancellationToken)
    {
        switch (targetType)
        {
            case ReactionTargetType.Post:
                var post = await _context.Posts.FirstAsync(p => p.Id == targetId, cancellationToken);
                post.IncrementReactionCount();
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