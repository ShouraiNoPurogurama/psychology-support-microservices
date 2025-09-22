/**
 * Get Following Query
 * ==================
 * 
 * Query to retrieve a paginated list of aliases that a specific alias is following.
 * This query supports pagination and returns following information with metadata.
 */

using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;

namespace Alias.API.Aliases.Features.GetFollowing;

/// <summary>
/// Query to get aliases that a specific alias is following with pagination support.
/// </summary>
/// <param name="AliasId">The ID of the alias to get following list for</param>
/// <param name="PageNumber">Page number for pagination (0-based)</param>
/// <param name="PageSize">Number of items per page (max 100)</param>
public sealed record GetFollowingQuery(
    Guid AliasId,
    int PageNumber = 0,
    int PageSize = 20
) : IQuery<GetFollowingResult>;

/// <summary>
/// Result containing paginated following information.
/// </summary>
/// <param name="Following">Paginated list of aliases being followed</param>
public sealed record GetFollowingResult(
    PaginatedResult<FollowingDto> Following
);

/// <summary>
/// DTO representing an alias that is being followed with follow metadata.
/// </summary>
/// <param name="AliasId">The ID of the followed alias</param>
/// <param name="Label">The display name of the followed alias</param>
/// <param name="AvatarMediaId">The avatar media ID of the followed alias (if any)</param>
/// <param name="FollowedAt">When the follow relationship was created</param>
/// <param name="IsVerified">Whether the followed alias is verified</param>
public sealed record FollowingDto(
    Guid AliasId,
    string Label,
    Guid? AvatarMediaId,
    DateTimeOffset FollowedAt,
    bool IsVerified
);
