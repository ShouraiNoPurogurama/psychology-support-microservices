/**
 * Unfollow Alias Command
 * =====================
 * 
 * Command to remove a follow relationship between the current user's alias and another alias.
 * This command handles the business logic of unfollowing an alias, including:
 * - Validating that the follow relationship exists
 * - Removing the follow relationship
 * - Updating follower/following counts on both aliases
 * - Publishing integration events for cross-service communication
 */

using BuildingBlocks.CQRS;

namespace Alias.API.Aliases.Features.UnfollowAlias;

/// <summary>
/// Command to unfollow an alias.
/// The follower alias ID is obtained from the current authenticated user context.
/// </summary>
/// <param name="FollowedAliasId">The ID of the alias to unfollow</param>
public sealed record UnfollowAliasCommand(
    Guid FollowedAliasId
) : ICommand<UnfollowAliasResult>;

/// <summary>
/// Result returned when successfully unfollowing an alias.
/// </summary>
/// <param name="FollowerAliasId">The ID of the alias that unfollowed</param>
/// <param name="FollowedAliasId">The ID of the alias that was unfollowed</param>
/// <param name="UnfollowedAt">The timestamp when the follow relationship was removed</param>
public sealed record UnfollowAliasResult(
    Guid FollowerAliasId,
    Guid FollowedAliasId,
    DateTimeOffset UnfollowedAt
);
