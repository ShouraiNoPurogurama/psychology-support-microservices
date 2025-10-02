using BuildingBlocks.CQRS;
using Microsoft.EntityFrameworkCore;
using Post.Application.Abstractions.Authentication;
using Post.Application.Abstractions.Integration;
using Post.Application.Data;
using Post.Domain.Aggregates.Reaction.DomainEvents;
using Post.Domain.Aggregates.Reactions.Enums;

namespace Post.Application.Features.Reactions.Commands.RemoveReaction;

public sealed class RemoveReactionCommandHandler : ICommandHandler<RemoveReactionCommand, RemoveReactionResult>
{
    private readonly IPostDbContext _context;
    private readonly IAliasVersionAccessor _aliasAccessor;
    private readonly ICurrentActorAccessor _currentActorAccessor;
    private readonly IOutboxWriter _outboxWriter;

    public RemoveReactionCommandHandler(
        IPostDbContext context,
        IAliasVersionAccessor aliasAccessor,
        IOutboxWriter outboxWriter, ICurrentActorAccessor currentActorAccessor)
    {
        _context = context;
        _aliasAccessor = aliasAccessor;
        _outboxWriter = outboxWriter;
        _currentActorAccessor = currentActorAccessor;
    }

    public async Task<RemoveReactionResult> Handle(RemoveReactionCommand request, CancellationToken cancellationToken)
    {
        var aliasContext = await _aliasAccessor.GetRequiredCurrentAliasVersionIdAsync(cancellationToken);

        var reaction = await _context.Reactions
            .Where(r => r.Target.TargetType == request.TargetType && 
                       r.Target.TargetId == request.TargetId &&
                       r.Author.AliasId == _currentActorAccessor.GetRequiredAliasId() &&
                       !r.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (reaction is null)
        {
            return new RemoveReactionResult(false, DateTimeOffset.UtcNow);
        }

        // Soft delete the reaction
        reaction.Delete(_currentActorAccessor.GetRequiredAliasId());

        // Update target counters
        await UpdateTargetCounters(request.TargetType, request.TargetId, cancellationToken);

        // Add domain event
        var reactionRemovedEvent = new ReactionRemovedEvent(
            reaction.Id,
            request.TargetType,
            request.TargetId,
            reaction.Type.Code,
            _currentActorAccessor.GetRequiredAliasId()
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
