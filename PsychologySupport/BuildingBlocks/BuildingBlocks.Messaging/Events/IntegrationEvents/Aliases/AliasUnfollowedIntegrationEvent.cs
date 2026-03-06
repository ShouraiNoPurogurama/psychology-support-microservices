/**
 * Alias Unfollowed Integration Event
 * =================================
 * 
 * Published when one alias unfollows another alias.
 * This event notifies other services about the removal of a follow relationship.
 * 
 * Used by:
 * - Feed Service: To update timeline algorithms and remove content from feeds
 * - Notification Service: To handle unfollow notifications if needed
 * - Analytics Service: To track social disengagement metrics
 */

using BuildingBlocks.Messaging.Events.IntegrationEvents;

namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Aliases;

/// <summary>
/// Integration event published when an alias unfollows another alias.
/// </summary>
/// <param name="FollowerAliasId">The ID of the alias that unfollowed</param>
/// <param name="FollowedAliasId">The ID of the alias that was unfollowed</param>
/// <param name="UnfollowedAt">The timestamp when the follow relationship was removed</param>
public record AliasUnfollowedIntegrationEvent(
    Guid FollowerAliasId,
    Guid FollowedAliasId,
    DateTimeOffset UnfollowedAt
) : IntegrationEvent;
