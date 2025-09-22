/**
 * Get Followers Query
 * ==================
 * 
 * Query to retrieve a paginated list of aliases that are following a specific alias.
 * This query supports pagination and returns follower information with metadata.
 */

using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;

namespace Alias.API.Aliases.Features.GetFollowers;

/// <summary>
/// Query to get followers of a specific alias with pagination support.
/// </summary>
/// <param name="AliasId">The ID of the alias to get followers for</param>
/// <param name="PageNumber">Page number for pagination (0-based)</param>
/// <param name="PageSize">Number of items per page (max 100)</param>
public sealed record GetFollowersQuery(
    Guid AliasId,
    int PageNumber = 0,
    int PageSize = 20
) : IQuery<GetFollowersResult>;

/// <summary>
/// Result containing paginated follower information.
/// </summary>
/// <param name="Followers">Paginated list of follower aliases</param>
public sealed record GetFollowersResult(
    PaginatedResult<FollowerDto> Followers
);

/// <summary>
/// DTO representing a follower alias with follow metadata.
/// </summary>
/// <param name="AliasId">The ID of the follower alias</param>
/// <param name="Label">The display name of the follower alias</param>
/// <param name="AvatarMediaId">The avatar media ID of the follower (if any)</param>
/// <param name="FollowedAt">When this follower started following</param>
/// <param name="IsVerified">Whether the follower alias is verified</param>
public sealed record FollowerDto(
    Guid AliasId,
    string Label,
    Guid? AvatarMediaId,
    DateTimeOffset FollowedAt,
    bool IsVerified
);
