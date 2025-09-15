using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Post.Application.Abstractions.Authentication;
using Post.Application.Data;
using Post.Application.Integration;
using Post.Domain.Aggregates.Reaction;
using Post.Domain.Aggregates.Reaction.ValueObjects;
using Post.Domain.Aggregates.Post.ValueObjects;
using Post.Domain.Events;
using Microsoft.EntityFrameworkCore;

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
            .Where(r => r.Target.TargetType == request.TargetType &&
                        r.Target.TargetId == request.TargetId &&
                        r.Author.AliasId == _actorResolver.AliasId &&
                        !r.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingReaction != null)
        {
            // Update existing reaction if different type
            if (existingReaction.Type.Code != request.ReactionCode)
            {
                var newReactionType = CreateReactionType(request.ReactionCode);
                existingReaction.UpdateType(newReactionType, _actorResolver.AliasId);

                var reactionUpdatedEvent = new ReactionUpdatedEvent(
                    existingReaction.Id,
                    request.TargetType,
                    request.TargetId,
                    request.ReactionCode,
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
                existingReaction.CreatedAt.Value
            );
        }

        // Create new reaction
        var reactionId = Guid.NewGuid();
        var target = ReactionTarget.Create(request.TargetType, request.TargetId);
        var reactionType = CreateReactionType(request.ReactionCode);
        var author = AuthorInfo.Create(_actorResolver.AliasId, aliasVersionId);

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

        // Update target counters
        await UpdateTargetCounters(request.TargetType, request.TargetId, cancellationToken);

        // Add domain event
        var reactionAddedEvent = new ReactionAddedEvent(
            reaction.Id,
            request.TargetType,
            request.TargetId,
            request.ReactionCode,
            _actorResolver.AliasId
        );
        await _outboxWriter.WriteAsync(reactionAddedEvent, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return new CreateReactionResult(
            reaction.Id,
            request.TargetType,
            request.TargetId,
            request.ReactionCode,
            reaction.CreatedAt.Value
        );
    }

    private async Task ValidateTargetExists(string targetType, Guid targetId, CancellationToken cancellationToken)
    {
        bool exists = targetType.ToLower() switch
        {
            "post" => await _context.Posts.AnyAsync(p => p.Id == targetId && !p.IsDeleted, cancellationToken),
            "comment" => await _context.Comments.AnyAsync(c => c.Id == targetId && !c.IsDeleted, cancellationToken),
            _ => throw new BadRequestException($"Invalid target type: {targetType}")
        };

        if (!exists)
        {
            throw new NotFoundException($"{targetType} with ID {targetId} not found");
        }
    }

    private async Task UpdateTargetCounters(string targetType, Guid targetId, CancellationToken cancellationToken)
    {
        switch (targetType.ToLower())
        {
            case "post":
                var post = await _context.Posts.FirstAsync(p => p.Id == targetId, cancellationToken);
                post.IncrementReactionCount();
                break;
        }
    }

    private static ReactionType CreateReactionType(string code)
    {
        return code.ToLower() switch
        {
            "like" => ReactionType.Create("like", "👍", 1),
            "heart" => ReactionType.Create("heart", "❤️", 2),
            "laugh" => ReactionType.Create("laugh", "😂", 1),
            "wow" => ReactionType.Create("wow", "😮", 1),
            "sad" => ReactionType.Create("sad", "😢", 1),
            "angry" => ReactionType.Create("angry", "😠", 1),
            _ => throw new BadRequestException($"Invalid reaction code: {code}")
        };
    }
}