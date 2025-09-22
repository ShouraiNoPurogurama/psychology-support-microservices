/**
 * Get Following Query Handler
 * ==========================
 * 
 * Handles retrieval of aliases that a specific alias is following with pagination.
 * This handler performs read-only operations and uses AsNoTracking for performance.
 */

using Alias.API.Aliases.Exceptions.DomainExceptions;
using Alias.API.Data.Public;
using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;

namespace Alias.API.Aliases.Features.GetFollowing;

/// <summary>
/// Handler for GetFollowingQuery that retrieves paginated following information.
/// </summary>
public sealed class GetFollowingHandler(
    AliasDbContext dbContext) : IQueryHandler<GetFollowingQuery, GetFollowingResult>
{
    public async Task<GetFollowingResult> Handle(GetFollowingQuery request, CancellationToken cancellationToken)
    {
        // Validate that the source alias exists and is not deleted
        var sourceAlias = await dbContext.Aliases
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == request.AliasId && !a.IsDeleted, cancellationToken);

        if (sourceAlias == null)
            throw new InvalidAliasDataException("Alias not found or has been deleted.");

        // Get total count of following
        var totalCount = await dbContext.Follows
            .AsNoTracking()
            .CountAsync(f => f.FollowerAliasId == request.AliasId, cancellationToken);

        // Get paginated following with alias information
        var following = await dbContext.Follows
            .AsNoTracking()
            .Where(f => f.FollowerAliasId == request.AliasId)
            .Join(dbContext.Aliases.AsNoTracking(),
                follow => follow.FollowedAliasId,
                alias => alias.Id,
                (follow, alias) => new FollowingDto(
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

        var paginatedResult = new PaginatedResult<FollowingDto>(
            request.PageNumber,
            request.PageSize,
            totalCount,
            following
            );

        return new GetFollowingResult(paginatedResult);
    }
}
