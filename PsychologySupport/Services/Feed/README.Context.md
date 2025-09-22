Feed Service Knowledge Context

Purpose
- Summarize architecture, key files, contracts, and behaviors in Feed service to onboard agents fast without re-reading sources.

Architecture & Projects
- Feed.API: Carter endpoints, auth wiring, Swagger, Cassandra session factory, JWT.
- Feed.Application: CQRS handlers (MediatR), validators, configuration objects, abstractions, MassTransit consumers.
- Feed.Domain: Domain entities/value objects for feed storage and moderation (Cassandra-oriented models).
- Feed.Infrastructure: Cassandra repositories, Redis services (ranking, trending, cursors, VIP), DI wiring.
- BuildingBlocks.*: Shared CQRS, Messaging (MassTransit setup), Observability, Redis connection helper.

API Layer (Carter)
- Program.cs: MapCarter(); UseSwagger(); UseAuthentication(); UseAuthorization(); loads config via Feed.API.Extensions.ConfigurationBuilderExtensions.
- DependencyInjection.cs: Adds Carter, JWT minimal validation, Cassandra ICluster/ISession, Swagger, CORS, Identity services.
- Endpoints/Feed/GetUserFeedEndpoint.cs
  - GET /api/v1/feed
  - Auth required. Extracts aliasId from JWT using ClaimsPrincipalExtensions.GetAliasId().
  - Query params: limit (int, default 20, clamped 1..100), cursor (string opaque).
  - Sends GetFeedQuery(aliasId, PageIndex: 0, PageSize: limit, Cursor: cursor) via MediatR.
  - Response DTO: GetUserFeedResponse { Items: List<UserFeedItemDto>, NextCursor, HasMore, TotalCount }.
- Extensions/ClaimsPrincipalExtensions.cs
  - GetAliasId(): reads claim "aliasId"; throws UnauthorizedAccessException if missing; parses Guid.
  - GetSubjectRef(): ClaimTypes.NameIdentifier Guid.

Application Layer
- Configuration/FeedConfiguration.cs
  - DefaultPageSize, MaxPageSize, VipFeedDays, TrendingTopN, FeedShardCount, VipCriteria, CacheSettings, RankDecayFactor (double, hours).
  - CacheSettings: VipCacheTtl, IdempotencyTtl, SnapshotTtl.
- Abstractions (interfaces)
  - CursorService/ICursorService: EncodeCursor(int offset, DateTime snapshotTs); DecodeCursor(string); ValidateCursor.
  - UserFeed/IUserFeedRepository:
    - AddFeedItemAsync(UserFeedItem)
    - RemoveFeedItemAsync(aliasId, ymdBucket, shard, postId)
    - GetFeedItemsAsync(aliasId, ymdBucket, shard, limit)
    - GetUserFeedAsync(aliasId, days, limit): iterates buckets and shards, merges/sorts.
  - ViewerFollowing/IViewerFollowingRepository: AddIfNotExists, Remove, Exists, GetAllByViewerAsync.
  - FollowerTracking/IFollowerTrackingRepository: AddIfNotExists, Remove, Exists, GetAllFollowersAsync(authorAliasId).
  - UserPinning/IUserPinningRepository: GetPinnedPostsAsync(aliasId, ct) (implementation in infra).
  - ViewerBlocking/IViewerBlockingRepository, ViewerMuting/IViewerMutingRepository: fetch per-viewer lists.
  - PostModeration/IPostModerationRepository: SuppressAsync, UnsuppressAsync, GetSuppressionAsync, IsCurrentlySuppressedAsync.
  - RankingService/IRankingService: GetTrendingPostsAsync(date), UpdatePostRankAsync, RankPostsAsync(followedAliasIds,trending,limit), AddToTrendingAsync.
  - VipService/IVipService: IsVipAsync, UpdateVipStatusAsync.
- Features/UserFeed/Queries/GetFeed
  - GetFeedQuery: (Guid AliasId, int PageIndex = 0, int PageSize = 20, string? Cursor = null) : IQuery<GetFeedResult>.
  - GetFeedResult: Items (List<UserFeedItemDto), NextCursor, HasMore, TotalCount.
  - UserFeedItemDto: PostId, YmdBucket, Shard, RankBucket, RankI64, TsUuid, CreatedAt, IsPinned.
  - GetFeedQueryValidator: validates AliasId, PageIndex >= 0, PageSize in 1..100, optional base64 cursor format.
  - GetFeedQueryHandler:
    - Dependencies: IUserFeedRepository, IViewerFollowingRepository, IUserPinningRepository, IViewerBlockingRepository, IViewerMutingRepository, IPostModerationRepository, IVipService, IRankingService, ICursorService, IOptions<FeedConfiguration>, IDistributedCache.
    - Caching: IDistributedCache snapshot per (aliasId, limit, cursor) key feed:resp:{aliasId}:{limit}:{cursor}. TTL from FeedConfiguration.Cache.SnapshotTtl.
    - Cursor: Decode/validate via ICursorService; if invalid or missing -> offset=0, snapshot=UtcNow; next_cursor encodes new offset & same snapshot.
    - VIP path (fan-out read): feedRepository.GetUserFeedAsync(aliasId, days: VipFeedDays, limit: PageSize*3). Apply suppression filter. First page includes pinned posts (as UserFeedItemDto with pinned flags). De-dup by PostId, then slice by offset/limit.
    - Regular path (fan-in read): Get follow list via IViewerFollowingRepository.GetAllByViewerAsync, trending via IRankingService.GetTrendingPostsAsync; rank via IRankingService.RankPostsAsync; apply suppression filter; first page adds pinned; de-dup and slice.
    - Filtering: currently only suppression check; TODO to apply block/mute once author info is available in feed items.
- Features/EventHandlers/Posts/PostApprovedIntegrationEventHandler.cs (MassTransit consumer)
  - Consumes BuildingBlocks.Messaging.Events.IntegrationEvents.Posts.PostApprovedIntegrationEvent(PostId, AuthorAliasId, ModeratorAliasId, ApprovedAt).
  - Fan-out for VIP authors: loads all followers via IFollowerTrackingRepository; if follower count < VipCriteria.MinFollowers -> return (fan-in path). Otherwise, distributes feed inserts across FeedShardCount using a simple hash (postId + followerId).
  - Adds UserFeedItem rows with recency-based rank (rankBucket=0, rankI64 descending by ticks), ymdBucket=today, tsUuid=new Guid, createdAt=ApprovedAt. Writes in batches (100) via IUserFeedRepository.AddFeedItemAsync.

Domain Layer
- Feed.Domain/UserFeed/UserFeedItem
  - Immutable creation via Create(aliasId, postId, optional ymdBucket, shard, rankBucket, rankI64, tsUuid, createdAt) with validations.
  - Fields: AliasId, YmdBucket (DateOnly), Shard (short), RankBucket (sbyte), RankI64 (long), TsUuid (Guid), PostId (Guid), CreatedAt (DateTimeOffset?).
- Feed.Domain/UserPinning/UserPinnedPost
  - Fields: AliasId, PinnedAt (Guid – used as TS UUID), PostId. Factory Create(aliasId, postId, pinnedAt?).
- Feed.Domain/PostModeration/PostSuppression
  - Stored via post_suppressed; repository exposes suppression and IsCurrentlySuppressed.
- Feed.Domain/FollowerTracking/Follower
  - AuthorAliasId, AliasId, Since.
- Feed.Domain/FeedConfiguration/FeedConfig
  - Generic key/value config record for dynamic settings; distinct from strongly-typed Feed.Application.Configuration.FeedConfiguration.

Infrastructure Layer
- DependencyInjection.cs
  - Registers Redis providers (IDatabase), Trending provider with decorators (logging, metrics, retry, key prefix).
  - Registers Cassandra PreparedStatementRegistry.
  - Adds Redis cache and multiplexer via BuildingBlocks.Extensions.AddRedisCache.
  - Registers service dependencies: VipService, RankingService, CursorService, repositories (UserFeed, FeedConfig, FollowerTracking, PostModeration, UserActivity, UserPinning, ViewerBlocking, ViewerFollowing, ViewerMuting, Idempotency).
- Persistence/Cassandra/Repositories/UserFeedRepository
  - Prepared statements for INSERT/SELECT/DELETE and select-for-delete (fetch clustering keys first, then delete by full PK).
  - AddFeedItemAsync: writes user_feed_by_bucket row with LocalQuorum/Quorum consistency; idempotent statements.
  - RemoveFeedItemAsync: find row by aliasId+ymd+shard+postId (ALLOW FILTERING), then delete by full PK.
  - GetFeedItemsAsync: fetch by aliasId+ymd+shard with ORDER BY rank_bucket DESC, rank_i64 DESC, ts_uuid DESC; limit param.
  - GetUserFeedAsync: for last N days, parallel query shards [0..FeedShardCount-1], collect and order by TsUuid desc; take limit.
- Persistence/Cassandra/Models/UserFeedByBucketRow
  - Partition keys: alias_id, ymd_bucket, shard. Clustering keys: rank_bucket, rank_i64, ts_uuid, post_id. created_at as column.
- Persistence/Cassandra/Repositories/FollowerTrackingRepository
  - Table followers_by_alias (author_alias_id, alias_id, since). Methods AddIfNotExists (LWT), Remove, Exists, GetAllFollowersAsync.
- Persistence/Cassandra/Repositories/ViewerFollowingRepository
  - Table follows_by_viewer (alias_id, followed_alias_id, since). Methods AddIfNotExists (LWT), Remove, Exists, GetAllByViewerAsync.
- Persistence/Cassandra/Repositories/PostModerationRepository
  - Table post_suppressed: insert suppression, delete unsuppress, select suppression, IsCurrentlySuppressedAsync wrapper.
- Data/Redis/RankingService (implements IRankingService)
  - Key patterns: rank:{postId}, trending:{yyyyMMdd}; stores rank data in hash (score, reactions, comments, ctr, updated_at) with TTL 7 days; trending as sorted set with 1-day TTL.
  - GetTrendingPostsAsync(date): top 100 Guids by score desc.
  - RankPostsAsync: merges followed alias IDs and trending post IDs (note: authorId/createdAt are placeholders if not cached), reads rank hash for each post, computes RankedPost with rankBucket=floor(score/10) and rankI64=score*1_000_000, returns top N by rankI64.
- Data/Redis/Providers/TrendingRedisProvider (implements ITrendingProvider)
  - Provides AddPostAsync/UpdatePostScoreAsync and GetTopPostsAsync for trending daily ZSET key.
- Data/Redis/CursorService (implements ICursorService)
  - CursorSecret read from configuration key Feed:CursorSecret; encodes cursor as base64(JSON) with HMACSHA256 signature and snapshot timestamp; provides Decode/Validate.
- Data/Redis/VipService (implements IVipService)
  - Simple String key vip:{aliasId} with TTL ~15m to cache VIP status.
- Data/Redis/RedisKeyPatterns
  - Helpers: vip:{aliasId}, rank:{postId}, trending:{date}, idem:{hash}, feed:reg:snap:{aliasId}:{snapshotTs}. Rank field name constants.

Messaging (Shared BuildingBlocks)
- BuildingBlocks.Messaging/MassTransit/Extensions.cs
  - AddMessageBroker(...) sets up MassTransit with RabbitMQ (host from configuration), in-memory outbox, kebab-case endpoints, scans consumers in provided assembly (Feed.Application passes its assembly).
- Shared Integration Events found
  - Posts/PostApprovedIntegrationEvent: record(PostId, AuthorAliasId, ModeratorAliasId, ApprovedAt). Used by fan-out consumer.
  - Other events not found in repo: FollowCreatedIntegrationEvent, ModerationEvaluatedIntegrationEvent, ReactionAddedIntegrationEvent, CommentCreatedIntegrationEvent. Feed features for these are pending shared contracts.

Security & Auth
- JWT validation via services.AddMinimalJwtValidation in Feed.API.Extensions.IdentityServiceExtensions.
- ClaimsPrincipalExtensions in Feed.API reads alias from claim type "aliasId".

Current Behavior Summary
- GET /api/v1/feed implements cursor-based pagination with Redis snapshot caching. First page includes pinned posts, then either:
  - VIP: read-optimized, fetch from Cassandra precomputed user feed (user_feed_by_bucket) across N days and shards, apply suppression filter, dedupe and page.
  - Regular: read trending + followed via Redis ranking service, apply suppression filter, merge pinned, dedupe and page.
- Post approval fan-out: If author is VIP (by follower count >= VipCriteria.MinFollowers), fan out to all followers by inserting UserFeedItem in Cassandra sharded by configured shard count. Otherwise do nothing (fan-in at read time).

Known Gaps / TODOs
- Event contracts missing (shared):
  - FollowCreatedIntegrationEvent: Needed to write follows_by_viewer and followers_by_alias; currently handled by repositories only when called locally.
  - ModerationEvaluatedIntegrationEvent: Needed to suppress/unsuppress posts in post_suppressed table.
  - ReactionAddedIntegrationEvent, CommentCreatedIntegrationEvent: Needed to enqueue UpdatePostRankCommand for background rank recomputation.
- Ranking re-write in Cassandra (rank updates):
  - To update rank keys for an existing feed item across many partitions (per follower), we need an index or projection that maps PostId -> (AliasId, ymd_bucket, shard, clustering keys). Not present. Current approach uses recency rank on insert; regular users’ ranking handled via Redis at read time.
- Block/mute filtering: Requires author alias in feed item or a lookup per post to filter; not currently available in Cassandra row or ranking result.

Operational Notes
- Cassandra queries use LocalQuorum/Quorum consistency and idempotent statements.
- Cursor snapshot uses HMAC secret at configuration key Feed:CursorSecret; ensure it’s set in environment/config.
- Redis cache connection configured via BuildingBlocks.Extensions.AddRedisCache (uses connection string "Redis").
- Swagger is enabled; in production uses /feed-service swagger endpoint path as configured.

Key File Map
- API
  - Feed.API/Program.cs
  - Feed.API/DependencyInjection.cs
  - Feed.API/Extensions/*.cs (ClaimsPrincipalExtensions, IdentityServiceExtensions)
  - Feed.API/Endpoints/Feed/GetUserFeedEndpoint.cs
- Application
  - Feed.Application/Configuration/FeedConfiguration.cs
  - Feed.Application/Abstractions/* (CursorService, UserFeed, Following, Pinning, Blocking, Muting, Moderation, RankingService, VipService, FollowerTracking, Redis providers)
  - Feed.Application/Features/UserFeed/Queries/GetFeed/* (Query, Handler, Validator, Result)
  - Feed.Application/Features/EventHandlers/Posts/PostApprovedIntegrationEventHandler.cs
- Domain
  - Feed.Domain/UserFeed/UserFeedItem.cs
  - Feed.Domain/UserPinning/UserPinnedPost.cs
  - Feed.Domain/FollowerTracking/Follower.cs
  - Feed.Domain/PostModeration/PostSuppression.cs
  - Feed.Domain/FeedConfiguration/FeedConfig.cs
- Infrastructure
  - Feed.Infrastructure/DependencyInjection.cs
  - Feed.Infrastructure/Persistence/Cassandra/Repositories/* (UserFeedRepository, FollowerTrackingRepository, ViewerFollowingRepository, PostModerationRepository, etc.)
  - Feed.Infrastructure/Persistence/Cassandra/Models/* (UserFeedByBucketRow, FollowersByAliasRow, FollowsByViewerRow, PostSuppressedRow, etc.)
  - Feed.Infrastructure/Data/Redis/* (RankingService, CursorService, VipService, Providers, RedisKeyPatterns)
- BuildingBlocks
  - BuildingBlocks/Extensions/ApplicationServicesExtensions.cs (AddRedisCache)
  - BuildingBlocks.Messaging/MassTransit/Extensions.cs (AddMessageBroker)
  - BuildingBlocks.Messaging/Events/IntegrationEvents/Posts/PostApprovedIntegrationEvent.cs

Quick Usage
- To fetch feed: call GET /api/v1/feed with Authorization: Bearer <JWT> and optional ?limit=&cursor=.
- To simulate VIP fan-out: publish PostApprovedIntegrationEvent for an author with >= VipCriteria.MinFollowers; observe user_feed_by_bucket writes for followers across shards.

Completion Criteria (for future changes)
- Endpoint returns sorted, filtered feed with pinned posts on first page; next_cursor present if more items exist.
- VIP fan-out on PostApprovedIntegrationEvent inserts feed items; regular authors skip fan-out.
- Caching works with TTL and precise keys; cursor decode/encode robust.
- No direct exposure of domain or persistence models through API; only DTOs.

