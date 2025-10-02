using System.Diagnostics;
using BuildingBlocks.CQRS;
using BuildingBlocks.Observability.Telemetry;
using Feed.Application.Abstractions.CursorService;
using Feed.Application.Abstractions.UserFeed;
using Feed.Application.Abstractions.ViewerFollowing;
using Feed.Application.Abstractions.UserPinning;
using Feed.Application.Abstractions.ViewerBlocking;
using Feed.Application.Abstractions.ViewerMuting;
using Feed.Application.Abstractions.PostModeration;
using Feed.Application.Abstractions.RankingService;
using Feed.Application.Abstractions.VipService;
using Feed.Application.Configuration;
using Microsoft.Extensions.Options;
using Mapster;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

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
    ICursorService cursorService,
    IOptions<FeedConfiguration> feedConfig,
    IDistributedCache cache)
    : IQueryHandler<GetFeedQuery, GetFeedResult>
{
    private readonly FeedConfiguration _config = feedConfig.Value;

    public async Task<GetFeedResult> Handle(GetFeedQuery request, CancellationToken cancellationToken)
    {
        using var activity = FeedActivitySource.Instance.StartActivity("feed.get_feed");
        activity?.SetTag("alias_id", request.AliasId);
        activity?.SetTag("page_size", request.PageSize);
        activity?.SetTag("cursor", request.Cursor ?? "");

        var sw = Stopwatch.StartNew();
        string feedType = "unknown";

        // Check cache snapshot first
        var cacheKey = BuildCacheKey(request.AliasId, request.PageSize, request.Cursor);
        var cached = await cache.GetStringAsync(cacheKey, cancellationToken);
        if (!string.IsNullOrEmpty(cached))
        {
            var hit = JsonSerializer.Deserialize<GetFeedResult>(cached);
            if (hit is not null)
            {
                activity?.SetTag("cache_hit", true);
                return hit;
            }
        }

        try
        {
            var isVip = await vipService.IsVipAsync(request.AliasId, cancellationToken);
            activity?.SetTag("is_vip", isVip);
            feedType = isVip ? "vip" : "regular";

            // Decode cursor to (offset, snapshot); if invalid -> start fresh
            var (offset, snapshot) = DecodeOrInitCursor(request.Cursor);

            var result = isVip
                ? await GetVipFeedAsync(request, offset, snapshot, cancellationToken)
                : await GetRegularFeedAsync(request, offset, snapshot, cancellationToken);

            // Cache the response snapshot
            var entryOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _config.Cache.SnapshotTtl
            };
            await cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(result), entryOptions, cancellationToken);

            FeedMetrics.FeedRequests.Add(1,
                new KeyValuePair<string, object?>("feed_type", feedType),
                new KeyValuePair<string, object?>("result", "success"));

            return result;
        }
        catch (Exception ex)
        {
            FeedMetrics.FeedRequests.Add(1,
                new KeyValuePair<string, object?>("feed_type", feedType),
                new KeyValuePair<string, object?>("result", "error"));

            activity?.SetTag("error", true);
            activity?.SetTag("error.message", ex.Message);
            throw;
        }
        finally
        {
            sw.Stop();
            FeedMetrics.FeedDuration.Record(sw.Elapsed.TotalMilliseconds,
                KeyValuePair.Create<string, object?>("feed_type", feedType));
        }
    }

    private static string BuildCacheKey(Guid aliasId, int limit, string? cursor)
        => $"feed:resp:{aliasId}:{limit}:{(cursor ?? "_")}";

    private (int Offset, DateTimeOffset Snapshot) DecodeOrInitCursor(string? cursor)
    {
        if (!string.IsNullOrEmpty(cursor))
        {
            try
            {
                var (offset, snap) = cursorService.DecodeCursor(cursor);
                return (offset, snap);
            }
            catch { /* ignore invalid cursor */ }
        }
        return (0, DateTimeOffset.UtcNow);
    }

    private async Task<GetFeedResult> GetVipFeedAsync(GetFeedQuery request, int offset, DateTimeOffset snapshot, CancellationToken cancellationToken)
    {
        using var activity = FeedActivitySource.Instance.StartActivity(FeedActivitySource.Operations.GetVipFeed);
        
        // VIP flow: pre-computed user_feed_by_bucket across recent days
        var feedItems = await feedRepository.GetUserFeedAsync(
            request.AliasId,
            days: _config.VipFeedDays,
            limit: request.PageSize * 3, // prefetch for filtering/pagination
            cancellationToken);

        var filteredItems = await ApplyFiltersAsync(request.AliasId, feedItems, cancellationToken);

        // Only include pinned on first page (cursor == null)
        var list = new List<UserFeedItemDto>();
        if (request.Cursor is null)
        {
            var pinnedPosts = await pinningRepository.GetPinnedPostsAsync(request.AliasId, cancellationToken);
            var pinnedDtos = pinnedPosts.Select(p => new UserFeedItemDto(
                p.PostId,
                DateOnly.FromDateTime(DateTime.UtcNow),
                0,
                100,
                long.MaxValue - DateTimeOffset.UtcNow.Ticks,
                p.PinnedAt,
                DateTimeOffset.UtcNow,
                IsPinned: true
            ));
            list.AddRange(pinnedDtos);
        }

        list.AddRange(filteredItems.Adapt<List<UserFeedItemDto>>());

        // de-dup by PostId preserving order
        var distinct = list
            .GroupBy(x => x.PostId)
            .Select(g => g.First())
            .ToList();

        var page = distinct.Skip(offset).Take(request.PageSize).ToList();
        var hasMore = distinct.Count > offset + page.Count;
        var nextCursor = hasMore ? cursorService.EncodeCursor(offset + page.Count, snapshot) : null;

        return new GetFeedResult(page, nextCursor, hasMore, distinct.Count);
    }

    private async Task<GetFeedResult> GetRegularFeedAsync(GetFeedQuery request, int offset, DateTimeOffset snapshot, CancellationToken cancellationToken)
    {
        using var activity = FeedActivitySource.Instance.StartActivity(FeedActivitySource.Operations.GetRegularFeed);
        
        // Regular flow: fan-in from followed aliases + trending
        var follows = await followingRepository.GetAllByViewerAsync(request.AliasId, cancellationToken);
        var followedAliasIds = follows.Select(f => f.FollowedAliasId).ToList();

        var trendingPosts = await rankingService.GetTrendingPostsAsync(DateTimeOffset.UtcNow, cancellationToken);

        var combinedPosts = await rankingService.RankPostsAsync(
            followedAliasIds,
            trendingPosts,
            request.PageSize * 3,
            cancellationToken);

        var filteredPosts = await ApplyPostFiltersAsync(request.AliasId, combinedPosts, cancellationToken);

        var items = new List<UserFeedItemDto>();
        if (request.Cursor is null)
        {
            var pinnedPosts = await pinningRepository.GetPinnedPostsAsync(request.AliasId, cancellationToken);
            var pinnedDtos = pinnedPosts.Select(p => new UserFeedItemDto(
                p.PostId,
                DateOnly.FromDateTime(DateTime.UtcNow),
                0,
                100,
                long.MaxValue - DateTimeOffset.UtcNow.Ticks,
                p.PinnedAt,
                DateTimeOffset.UtcNow,
                true
            ));
            items.AddRange(pinnedDtos);
        }

        items.AddRange(filteredPosts.Select(p => new UserFeedItemDto(
            p.PostId,
            DateOnly.FromDateTime(p.CreatedAt.Date),
            0,
            p.RankBucket,
            p.RankI64,
            Guid.NewGuid(),
            p.CreatedAt
        )));

        var distinct = items.GroupBy(i => i.PostId).Select(g => g.First()).ToList();
        var page = distinct.Skip(offset).Take(request.PageSize).ToList();
        var hasMore = distinct.Count > offset + page.Count;
        var nextCursor = hasMore ? cursorService.EncodeCursor(offset + page.Count, snapshot) : null;

        return new GetFeedResult(page, nextCursor, hasMore, distinct.Count);
    }

    private async Task<IReadOnlyList<Domain.UserFeed.UserFeedItem>> ApplyFiltersAsync(
        Guid aliasId,
        IReadOnlyList<Domain.UserFeed.UserFeedItem> items,
        CancellationToken cancellationToken)
    {
        if (items.Count == 0)
            return items;

        // OPTIMIZED: Batch query instead of N+1 queries
        var postIds = items.Select(i => i.PostId).ToList();
        var suppressedIds = await moderationRepository.GetSuppressedPostIdsBatchAsync(postIds, cancellationToken);

        // Filter out suppressed posts
        var filtered = items.Where(item => !suppressedIds.Contains(item.PostId)).ToList();

        // TODO: When author alias is available on feed item, also filter by blocked/muted authors
        return filtered;
    }

    private async Task<IReadOnlyList<RankedPost>> ApplyPostFiltersAsync(
        Guid aliasId,
        IReadOnlyList<RankedPost> posts,
        CancellationToken cancellationToken)
    {
        if (posts.Count == 0)
            return posts;

        // OPTIMIZED: Batch query instead of N+1 queries
        var postIds = posts.Select(p => p.PostId).ToList();
        var suppressedIds = await moderationRepository.GetSuppressedPostIdsBatchAsync(postIds, cancellationToken);

        // Filter out suppressed posts
        var filtered = posts.Where(post => !suppressedIds.Contains(post.PostId)).ToList();

        // TODO: Filter out posts from blocked/muted authors when AuthorAliasId is present
        return filtered;
    }
}

// Supporting types for regular feed
