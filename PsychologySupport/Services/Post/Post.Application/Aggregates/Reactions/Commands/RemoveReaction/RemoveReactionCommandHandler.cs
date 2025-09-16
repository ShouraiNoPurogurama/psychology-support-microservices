using BuildingBlocks.CQRS;
using Post.Application.Abstractions.Authentication;
using Post.Application.Data;
using Post.Application.Integration;
using Microsoft.EntityFrameworkCore;
using Post.Domain.Aggregates.Reaction.DomainEvents;
using Post.Domain.Aggregates.Reactions.Enums;

namespace Post.Application.Aggregates.Reactions.Commands.RemoveReaction;

internal sealed class RemoveReactionCommandHandler : ICommandHandler<RemoveReactionCommand, RemoveReactionResult>
{
    private readonly IPostDbContext _context;
    private readonly IAliasVersionResolver _aliasResolver;
    private readonly IActorResolver _actorResolver;
    private readonly IOutboxWriter _outboxWriter;

    public RemoveReactionCommandHandler(
        IPostDbContext context,
        IAliasVersionResolver aliasResolver,
        IOutboxWriter outboxWriter, IActorResolver actorResolver)
    {
        _context = context;
        _aliasResolver = aliasResolver;
        _outboxWriter = outboxWriter;
        _actorResolver = actorResolver;
    }

    public async Task<RemoveReactionResult> Handle(RemoveReactionCommand request, CancellationToken cancellationToken)
    {
        var aliasContext = await _aliasResolver.GetCurrentAliasVersionIdAsync(cancellationToken);

        var reaction = await _context.Reactions
            .Where(r => r.Target.TargetType == request.TargetType && 
                       r.Target.TargetId == request.TargetId &&
                       r.Author.AliasId == _actorResolver.AliasId &&
                       !r.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (reaction is null)
        {
            return new RemoveReactionResult(false, DateTime.UtcNow);
        }

        // Soft delete the reaction
        reaction.Delete(_actorResolver.AliasId);

        // Update target counters
        await UpdateTargetCounters(request.TargetType, request.TargetId, cancellationToken);

        // Add domain event
        var reactionRemovedEvent = new ReactionRemovedEvent(
            reaction.Id,
            request.TargetType,
            request.TargetId,
            reaction.Type.Code,
            _actorResolver.AliasId
        );
        await _outboxWriter.WriteAsync(reactionRemovedEvent, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return new RemoveReactionResult(true, reaction.DeletedAt!.Value);
    }

    private async Task UpdateTargetCounters(ReactionTargetType targetType, Guid targetId, CancellationToken cancellationToken)
    {
        switch (targetType)
        {
            case ReactionTargetType.Post:
                var post = await _context.Posts.FirstAsync(p => p.Id == targetId, cancellationToken);
                post.DecrementReactionCount();
                break;
        }
    }
}
