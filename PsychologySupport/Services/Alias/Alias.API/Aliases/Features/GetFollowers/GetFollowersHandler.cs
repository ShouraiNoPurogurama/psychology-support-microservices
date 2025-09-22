/**
 * Get Followers Query Handler
 * ==========================
 * 
 * Handles retrieval of followers for a specific alias with pagination.
 * This handler performs read-only operations and uses AsNoTracking for performance.
 */

using Alias.API.Aliases.Exceptions.DomainExceptions;
using Alias.API.Data.Public;
using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Mapster;

namespace Alias.API.Aliases.Features.GetFollowers;

/// <summary>
/// Handler for GetFollowersQuery that retrieves paginated follower information.
/// </summary>
public sealed class GetFollowersHandler(
    AliasDbContext dbContext) : IQueryHandler<GetFollowersQuery, GetFollowersResult>
{
    public async Task<GetFollowersResult> Handle(GetFollowersQuery request, CancellationToken cancellationToken)
    {
        // Validate that the target alias exists and is not deleted
        var targetAlias = await dbContext.Aliases
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == request.AliasId && !a.IsDeleted, cancellationToken);

        if (targetAlias == null)
            throw new InvalidAliasDataException("Alias not found or has been deleted.");

        // Get total count of followers
        var totalCount = await dbContext.Follows
            .AsNoTracking()
            .CountAsync(f => f.FollowedAliasId == request.AliasId, cancellationToken);

        // Get paginated followers with alias information
        var followers = await dbContext.Follows
            .AsNoTracking()
            .Where(f => f.FollowedAliasId == request.AliasId)
            .Join(dbContext.Aliases.AsNoTracking(),
                follow => follow.FollowerAliasId,
                alias => alias.Id,
                (follow, alias) => new FollowerDto(
                    alias.Id,
                    alias.Label.Value,
                    alias.AvatarMediaId,
                    follow.FollowedAt,
                    alias.Metadata.IsSystemGenerated // Using this as a simple "verified" indicator
                ))
            .OrderByDescending(f => f.FollowedAt)
            .Skip(request.PageNumber * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var paginatedResult = new PaginatedResult<FollowerDto>(
            request.PageNumber,
            request.PageSize,
            totalCount,
            followers);

        return new GetFollowersResult(paginatedResult);
    }
}
