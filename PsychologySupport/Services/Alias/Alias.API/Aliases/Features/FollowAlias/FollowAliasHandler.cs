/**
 * Follow Alias Command Handler
 * ===========================
 * 
 * Handles the business logic for creating a follow relationship between two aliases.
 * This handler implements all the business rules and ensures data consistency.
 * 
 * Business Rules Implemented:
 * - An alias cannot follow itself
 * - Cannot follow an alias that is already being followed
 * - Cannot follow suspended, banned, or deleted aliases
 * - Updates follower/following counts atomically
 * - Publishes integration events for other services
 */

using Alias.API.Aliases.Exceptions.DomainExceptions;
using Alias.API.Aliases.Models;
using Alias.API.Aliases.Models.Aliases.Enums;
using Alias.API.Aliases.Models.Follows;
using Alias.API.Common.Authentication;
using Alias.API.Data.Public;
using BuildingBlocks.CQRS;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Alias;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Aliases;
using Microsoft.EntityFrameworkCore;
using MassTransit;

namespace Alias.API.Aliases.Features.FollowAlias;

/// <summary>
/// Handler for the FollowAliasCommand that manages follow relationship creation.
/// </summary>
public sealed class FollowAliasHandler(
    AliasDbContext dbContext,
    ICurrentActorAccessor currentActorAccessor,
    IPublishEndpoint publishEndpoint) : ICommandHandler<FollowAliasCommand, FollowAliasResult>
{
    public async Task<FollowAliasResult> Handle(FollowAliasCommand request, CancellationToken cancellationToken)
    {
        // Get current user's alias ID from authentication context
        var followerAliasId = currentActorAccessor.GetRequiredAliasId();

        // Business rule: Cannot follow yourself
        if (followerAliasId == request.FollowedAliasId)
            throw new InvalidAliasDataException("An alias cannot follow itself.");

        // Check if follow relationship already exists
        var existingFollow = await dbContext.Follows
            .FirstOrDefaultAsync(f => 
                f.FollowerAliasId == followerAliasId && 
                f.FollowedAliasId == request.FollowedAliasId, 
                cancellationToken);

        if (existingFollow != null)
            throw new InvalidAliasDataException("Already following this alias.");

        // Validate that both aliases exist and can participate in follow relationships
        var followerAlias = await dbContext.Aliases
            .FirstOrDefaultAsync(a => a.Id == followerAliasId && !a.IsDeleted, cancellationToken);

        if (followerAlias == null)
            throw new InvalidAliasDataException("Follower alias not found or has been deleted.");

        var followedAlias = await dbContext.Aliases
            .FirstOrDefaultAsync(a => a.Id == request.FollowedAliasId && !a.IsDeleted, cancellationToken);

        if (followedAlias == null)
            throw new InvalidAliasDataException("Target alias not found or has been deleted.");

        // Business rule: Cannot follow suspended or banned aliases
        if (followedAlias.Status == AliasStatus.Suspended)
            throw new InvalidAliasDataException("Cannot follow a suspended alias.");

        if (followedAlias.Status == AliasStatus.Banned)
            throw new InvalidAliasDataException("Cannot follow a banned alias.");

        // Business rule: Cannot follow private aliases unless explicitly allowed
        if (followedAlias.Visibility == AliasVisibility.Private)
            throw new InvalidAliasDataException("Cannot follow a private alias.");

        // Create the follow relationship
        var follow = Follow.Create(followerAliasId, request.FollowedAliasId);

        // Update follower counts on both aliases
        followerAlias.IncrementFollowingCount();
        followedAlias.IncrementFollowersCount();

        // Persist changes to database
        dbContext.Follows.Add(follow);
        await dbContext.SaveChangesAsync(cancellationToken);

        // Publish integration event for other services
        var integrationEvent = new AliasFollowedIntegrationEvent(
            followerAliasId,
            request.FollowedAliasId,
            follow.FollowedAt);

        await publishEndpoint.Publish(integrationEvent, cancellationToken);

        return new FollowAliasResult(
            follow.Id,
            followerAliasId,
            request.FollowedAliasId,
            follow.FollowedAt);
    }
}
