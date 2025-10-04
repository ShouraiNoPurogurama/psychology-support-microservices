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
using Feed.Application.Abstractions.PostRepository;
using Feed.Application.Abstractions.RankingService;
using Feed.Application.Abstractions.VipService;
using Feed.Application.Configuration;
using Microsoft.Extensions.Options;
using Mapster;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
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
    IPostReadRepository postReadRepository,
    IOptions<FeedConfiguration> feedConfig,
    IDistributedCache cache,
    ILogger<GetFeedQueryHandler> logger)
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
        
        // Regular flow: fan-in from followed aliases + trending with multi-tiered fallback
        var follows = await followingRepository.GetAllByViewerAsync(request.AliasId, cancellationToken);
        var followedAliasIds = follows.Select(f => f.FollowedAliasId).ToList();

        // Primary: Try to get daily trending posts
        var trendingPosts = await rankingService.GetTrendingPostsAsync(DateTimeOffset.UtcNow, cancellationToken);
        
        // Multi-tiered fallback mechanism
        if (trendingPosts.Count == 0)
        {
            activity?.SetTag("fallback_triggered", true);
            activity?.SetTag("fallback_reason", "daily_trending_empty");
            logger.LogInformation("Daily trending is empty for user {AliasId}, triggering fallback mechanism", request.AliasId);
            
            // Tier 1 Fallback: Try personalized fallback (category-based)
            var personalizedFallback = await rankingService.GetPersonalizedFallbackPostsAsync(
                request.AliasId, 
                100, 
                cancellationToken);
            
            if (personalizedFallback.Count > 0)
            {
                trendingPosts = personalizedFallback;
                activity?.SetTag("fallback_tier", "personalized");
                activity?.AddEvent(new System.Diagnostics.ActivityEvent("Using personalized fallback posts"));
                logger.LogInformation("Using personalized fallback for user {AliasId}, found {Count} posts", 
                    request.AliasId, personalizedFallback.Count);
                FeedMetrics.FeedRequests.Add(1,
                    new KeyValuePair<string, object?>("feed_type", "regular"),
                    new KeyValuePair<string, object?>("fallback_tier", "personalized"));
            }
            else
            {
                // Tier 2 Fallback: Global fallback from trending:global_fallback
                var globalFallback = await rankingService.GetGlobalFallbackPostsAsync(100, cancellationToken);
                
                if (globalFallback.Count > 0)
                {
                    trendingPosts = globalFallback;
                    activity?.SetTag("fallback_tier", "global");
                    activity?.AddEvent(new System.Diagnostics.ActivityEvent("Using global fallback posts"));
                    logger.LogInformation("Using global fallback for user {AliasId}, found {Count} posts", 
                        request.AliasId, globalFallback.Count);
                    FeedMetrics.FeedRequests.Add(1,
                        new KeyValuePair<string, object?>("feed_type", "regular"),
                        new KeyValuePair<string, object?>("fallback_tier", "global"));
                }
                else
                {
                    // Tier 3 Fallback: Database query (deepest fallback - slow path)
                    logger.LogWarning("Global fallback is empty for user {AliasId}, attempting database fallback (slow path)", 
                        request.AliasId);
                    
                    var dbFallback = await postReadRepository.GetMostRecentPublicPostsAsync(100, cancellationToken);
                    
                    if (dbFallback.Count > 0)
                    {
                        trendingPosts = dbFallback;
                        activity?.SetTag("fallback_tier", "database");
                        activity?.AddEvent(new System.Diagnostics.ActivityEvent("Using database fallback posts (SLOW PATH)"));
                        logger.LogWarning("Using database fallback for user {AliasId}, found {Count} posts. This is a SLOW PATH - populate global fallback!", 
                            request.AliasId, dbFallback.Count);
                        FeedMetrics.FeedRequests.Add(1,
                            new KeyValuePair<string, object?>("feed_type", "regular"),
                            new KeyValuePair<string, object?>("fallback_tier", "database"));
                    }
                    else
                    {
                        // All fallbacks exhausted
                        logger.LogWarning("All fallback tiers exhausted for user {AliasId}, returning feed with only followed posts", 
                            request.AliasId);
                        activity?.SetTag("fallback_tier", "none");
                        activity?.AddEvent(new System.Diagnostics.ActivityEvent("All fallback tiers exhausted"));
                    }
                }
            }
        }
        else
        {
            activity?.SetTag("fallback_triggered", false);
            logger.LogDebug("Daily trending available for user {AliasId}, found {Count} posts", 
                request.AliasId, trendingPosts.Count);
        }

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

        // OPTIMIZED: Batch query for suppression check
        var postIds = items.Select(i => i.PostId).ToList();
        var suppressedIds = await moderationRepository.GetSuppressedPostIdsBatchAsync(postIds, cancellationToken);

        // Get blocked aliases for the viewer
        var blockedUsers = await blockingRepository.GetAllByViewerAsync(aliasId, cancellationToken);
        var blockedAliasIds = blockedUsers.Select(b => b.BlockedAliasId).ToHashSet();

        // Get muted aliases for the viewer
        var mutedUsers = await mutingRepository.GetAllByViewerAsync(aliasId, cancellationToken);
        var mutedAliasIds = mutedUsers.Select(m => m.MutedAliasId).ToHashSet();

        // Get author IDs for all posts (batch query to Redis)
        var postAuthorTasks = postIds.Select(async postId =>
        {
            var authorId = await rankingService.GetPostAuthorAsync(postId, cancellationToken);
            return (postId, authorId);
        });
        var postAuthors = await Task.WhenAll(postAuthorTasks);
        var postAuthorMap = postAuthors
            .Where(x => x.authorId.HasValue)
            .ToDictionary(x => x.postId, x => x.authorId!.Value);

        // Apply all filters
        var filtered = items.Where(item =>
        {
            // Filter out suppressed posts
            if (suppressedIds.Contains(item.PostId))
                return false;

            // Filter out posts from blocked users
            if (postAuthorMap.TryGetValue(item.PostId, out var authorId))
            {
                if (blockedAliasIds.Contains(authorId))
                    return false;

                if (mutedAliasIds.Contains(authorId))
                    return false;
            }

            return true;
        }).ToList();

        return filtered;
    }

    private async Task<IReadOnlyList<RankedPost>> ApplyPostFiltersAsync(
        Guid aliasId,
        IReadOnlyList<RankedPost> posts,
        CancellationToken cancellationToken)
    {
        if (posts.Count == 0)
            return posts;

        // OPTIMIZED: Batch query for suppression check
        var postIds = posts.Select(p => p.PostId).ToList();
        var suppressedIds = await moderationRepository.GetSuppressedPostIdsBatchAsync(postIds, cancellationToken);

        // Get blocked aliases for the viewer
        var blockedUsers = await blockingRepository.GetAllByViewerAsync(aliasId, cancellationToken);
        var blockedAliasIds = blockedUsers.Select(b => b.BlockedAliasId).ToHashSet();

        // Get muted aliases for the viewer
        var mutedUsers = await mutingRepository.GetAllByViewerAsync(aliasId, cancellationToken);
        var mutedAliasIds = mutedUsers.Select(m => m.MutedAliasId).ToHashSet();

        // Apply all filters
        var filtered = posts.Where(post =>
        {
            // Filter out suppressed posts
            if (suppressedIds.Contains(post.PostId))
                return false;

            // Filter out posts from blocked users (RankedPost has AuthorAliasId)
            if (blockedAliasIds.Contains(post.AuthorAliasId))
                return false;

            if (mutedAliasIds.Contains(post.AuthorAliasId))
                return false;

            return true;
        }).ToList();

        return filtered;
    }
}

// Supporting types for regular feed
