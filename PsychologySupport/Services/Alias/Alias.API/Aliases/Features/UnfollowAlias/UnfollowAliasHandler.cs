/**
 * Unfollow Alias Command Handler
 * =============================
 * 
 * Handles the business logic for removing a follow relationship between two aliases.
 * This handler ensures data consistency and publishes appropriate events.
 * 
 * Business Rules Implemented:
 * - Can only unfollow if a follow relationship exists
 * - Updates follower/following counts atomically
 * - Publishes integration events for other services
 * - Handles domain events from the Follow aggregate
 */

using Alias.API.Aliases.Exceptions.DomainExceptions;
using Alias.API.Common.Authentication;
using Alias.API.Common.Outbox;
using Alias.API.Data.Public;
using BuildingBlocks.CQRS;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Alias;
using Microsoft.EntityFrameworkCore;

namespace Alias.API.Aliases.Features.UnfollowAlias;

/// <summary>
/// Handler for the UnfollowAliasCommand that manages follow relationship removal.
/// </summary>
public sealed class UnfollowAliasHandler(
    AliasDbContext dbContext,
    ICurrentActorAccessor currentActorAccessor,
    IOutboxWriter outbox) : ICommandHandler<UnfollowAliasCommand, UnfollowAliasResult>
{
    public async Task<UnfollowAliasResult> Handle(UnfollowAliasCommand request, CancellationToken cancellationToken)
    {
        // Get current user's alias ID from authentication context
        var followerAliasId = currentActorAccessor.GetRequiredAliasId();

        // Find the existing follow relationship
        var follow = await dbContext.Follows
            .FirstOrDefaultAsync(f => 
                f.FollowerAliasId == followerAliasId && 
                f.FollowedAliasId == request.FollowedAliasId, 
                cancellationToken)
            ?? throw new InvalidAliasDataException("Follow relationship does not exist.");

        // Get both aliases to update their counts
        var followerAlias = await dbContext.Aliases
            .FirstOrDefaultAsync(a => a.Id == followerAliasId && !a.IsDeleted, cancellationToken)
            ?? throw new InvalidAliasDataException("Follower alias not found or has been deleted.");

        var followedAlias = await dbContext.Aliases
            .FirstOrDefaultAsync(a => a.Id == request.FollowedAliasId && !a.IsDeleted, cancellationToken)
            ?? throw new InvalidAliasDataException("Followed alias not found or has been deleted.");

        // Call domain method to handle removal logic and events
        follow.Remove();

        // Update follower counts on both aliases
        followerAlias.DecrementFollowingCount();
        followedAlias.DecrementFollowersCount();

        // Remove the follow relationship from database
        dbContext.Follows.Remove(follow);

        // Outbox event before save
        await outbox.WriteAsync(new UserUnfollowedIntegrationEvent(
            followerAliasId,
            request.FollowedAliasId
        ), cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        var unfollowedAt = DateTimeOffset.UtcNow;
        return new UnfollowAliasResult(
            followerAliasId,
            request.FollowedAliasId,
            unfollowedAt);
    }
}
