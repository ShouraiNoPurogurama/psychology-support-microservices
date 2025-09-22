/**
 * Follow Alias Command
 * ===================
 * 
 * Command to create a follow relationship between the current user's alias and another alias.
 * This command handles the business logic of following another alias, including:
 * - Validating that the target alias exists and can be followed
 * - Creating the follow relationship
 * - Updating follower/following counts on both aliases
 * - Publishing integration events for cross-service communication
 */

using BuildingBlocks.CQRS;

namespace Alias.API.Aliases.Features.FollowAlias;

/// <summary>
/// Command to follow another alias.
/// The follower alias ID is obtained from the current authenticated user context.
/// </summary>
/// <param name="FollowedAliasId">The ID of the alias to follow</param>
public sealed record FollowAliasCommand(
    Guid FollowedAliasId
) : ICommand<FollowAliasResult>;

/// <summary>
/// Result returned when successfully following an alias.
/// </summary>
/// <param name="FollowId">The unique identifier of the created follow relationship</param>
/// <param name="FollowerAliasId">The ID of the alias that is now following</param>
/// <param name="FollowedAliasId">The ID of the alias being followed</param>
/// <param name="FollowedAt">The timestamp when the follow relationship was created</param>
public sealed record FollowAliasResult(
    Guid FollowId,
    Guid FollowerAliasId,
    Guid FollowedAliasId,
    DateTimeOffset FollowedAt
);
