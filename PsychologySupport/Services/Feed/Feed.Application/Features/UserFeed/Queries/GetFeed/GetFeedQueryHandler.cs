using BuildingBlocks.CQRS;
using Feed.Application.Abstractions.CursorService;
using Feed.Application.Abstractions.UserFeed;
using Feed.Application.Abstractions.ViewerFollowing;
using Feed.Application.Abstractions.UserPinning;
using Feed.Application.Abstractions.ViewerBlocking;
using Feed.Application.Abstractions.ViewerMuting;
using Feed.Application.Abstractions.PostModeration;
using Feed.Application.Abstractions.RankingService;
using Feed.Application.Abstractions.VipService;
using Mapster;

namespace Feed.Application.Features.UserFeed.Queries.GetFeed;

public sealed class GetFeedQueryHandler(
    IUserFeedRepository feedRepository,
    IViewerFollowingRepository followingRepository,
    IUserPinningRepository pinningRepository,
    IViewerBlockingRepository blockingRepository,
    IViewerMutingRepository mutingRepository,
    IPostModerationRepository moderationRepository,
    IVipService vipService,
    IRankingService rankingService,
    ICursorService cursorService)
    : IQueryHandler<GetFeedQuery, GetFeedResult>
{
    public async Task<GetFeedResult> Handle(GetFeedQuery request, CancellationToken cancellationToken)
    {
        // Check if user is VIP
        var isVip = await vipService.IsVipAsync(request.AliasId, cancellationToken);

        if (isVip)
        {
            return await GetVipFeedAsync(request, cancellationToken);
        }
        else
        {
            return await GetRegularFeedAsync(request, cancellationToken);
        }
    }

    private async Task<GetFeedResult> GetVipFeedAsync(GetFeedQuery request, CancellationToken cancellationToken)
    {
        // VIP flow: Query from pre-computed user_feed_by_bucket
        var feedItems = await feedRepository.GetUserFeedAsync(
            request.AliasId,
            days: 7,
            limit: request.PageSize * 2, // Get extra for filtering
            cancellationToken);

        // Apply filters (blocked, muted, suppressed)
        var filteredItems = await ApplyFiltersAsync(request.AliasId, feedItems, cancellationToken);

        // Get pinned posts and merge on top
        var pinnedPosts = await pinningRepository.GetPinnedPostsAsync(request.AliasId, cancellationToken);
        var pinnedDtos = pinnedPosts.Select(p => new UserFeedItemDto(
            p.PostId,
            DateOnly.FromDateTime(DateTime.UtcNow),
            0,
            100, // High rank for pinned posts
            0,
            p.PinnedAt,
            null,
            IsPinned: true
        )).ToList();

        // Merge and paginate
        var allItems = pinnedDtos.Concat(filteredItems.Adapt<List<UserFeedItemDto>>()).ToList();
        var paginatedItems = allItems
            .Skip(request.PageIndex * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var nextCursor = paginatedItems.Count == request.PageSize 
            ? cursorService.EncodeCursor(request.PageIndex + 1, DateTime.UtcNow)
            : null;

        return new GetFeedResult(
            paginatedItems,
            nextCursor,
            paginatedItems.Count == request.PageSize,
            allItems.Count
        );
    }

    private async Task<GetFeedResult> GetRegularFeedAsync(GetFeedQuery request, CancellationToken cancellationToken)
    {
        // Regular flow: Fan-in from follows + trending
        var follows = await followingRepository.GetAllByViewerAsync(request.AliasId, cancellationToken);
        var followedAliasIds = follows.Select(f => f.FollowedAliasId).ToList();

        // Get trending posts from Redis
        var trendingPosts = await rankingService.GetTrendingPostsAsync(DateTime.UtcNow, cancellationToken);

        // Combine and rank posts
        var combinedPosts = await rankingService.RankPostsAsync(
            followedAliasIds, 
            trendingPosts, 
            request.PageSize * 2,
            cancellationToken);

        // Apply filters
        var filteredPosts = await ApplyPostFiltersAsync(request.AliasId, combinedPosts, cancellationToken);

        // Convert to DTOs
        var feedItems = filteredPosts.Select(p => new UserFeedItemDto(
            p.PostId,
            DateOnly.FromDateTime(p.CreatedAt.Date),
            0,
            p.RankBucket,
            p.RankI64,
            Guid.NewGuid(), // Generate for regular posts
            p.CreatedAt
        )).ToList();

        var paginatedItems = feedItems
            .Skip(request.PageIndex * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var nextCursor = paginatedItems.Count == request.PageSize
            ? cursorService.EncodeCursor(request.PageIndex + 1, DateTime.UtcNow)
            : null;

        return new GetFeedResult(
            paginatedItems,
            nextCursor,
            paginatedItems.Count == request.PageSize,
            feedItems.Count
        );
    }

    private async Task<IReadOnlyList<Domain.UserFeed.UserFeedItem>> ApplyFiltersAsync(
        Guid aliasId, 
        IReadOnlyList<Domain.UserFeed.UserFeedItem> items, 
        CancellationToken cancellationToken)
    {
        // Get filter sets
        var blockedAliases = await blockingRepository.GetAllByViewerAsync(aliasId, cancellationToken);
        var mutedAliases = await mutingRepository.GetAllByViewerAsync(aliasId, cancellationToken);
        
        var blockedSet = blockedAliases.Select(b => b.BlockedAliasId).ToHashSet();
        var mutedSet = mutedAliases.Select(m => m.MutedAliasId).ToHashSet();

        var filtered = new List<Domain.UserFeed.UserFeedItem>();

        foreach (var item in items)
        {
            // Check if post is suppressed
            var suppression = await moderationRepository.GetSuppressionAsync(item.PostId, cancellationToken);
            if (suppression?.IsCurrentlySuppressed == true)
                continue;

            // Apply blocking/muting filters (would need post author info)
            // For now, just add all non-suppressed posts
            filtered.Add(item);
        }

        return filtered;
    }

    private async Task<IReadOnlyList<RankedPost>> ApplyPostFiltersAsync(
        Guid aliasId, 
        IReadOnlyList<RankedPost> posts, 
        CancellationToken cancellationToken)
    {
        // Similar filtering logic for regular feed posts
        return posts; // Simplified for now
    }
}

// Supporting types for regular feed
