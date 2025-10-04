using BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;
using Feed.Application.Abstractions.FanOut;
using Feed.Application.Abstractions.FollowerTracking;
using Feed.Application.Abstractions.PostRepository;
using Feed.Application.Abstractions.RankingService;
using Feed.Application.Configuration;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Feed.Application.Features.Consumers.Post;

/// <summary>
/// Consumes PostPublishedIntegrationEvent and fans out the post to all followers.
/// Only performs fan-out if the author is a VIP (has enough followers).
/// Also syncs the published post to Cassandra replica tables if it's public and finalized.
/// </summary>
public sealed class PostPublishedIntegrationEventConsumer : IConsumer<PostPublishedIntegrationEvent>
{
    private readonly IFeedFanOutService _fanOutService;
    private readonly IFollowerTrackingRepository _followerRepo;
    private readonly IPostReplicaRepository _postReplicaRepository;
    private readonly IRankingService _rankingService;
    private readonly FeedConfiguration _config;
    private readonly ILogger<PostPublishedIntegrationEventConsumer> _logger;

    public PostPublishedIntegrationEventConsumer(
        IFeedFanOutService fanOutService,
        IFollowerTrackingRepository followerRepo,
        IPostReplicaRepository postReplicaRepository,
        IRankingService rankingService,
        IOptions<FeedConfiguration> options,
        ILogger<PostPublishedIntegrationEventConsumer> logger)
    {
        _fanOutService = fanOutService;
        _followerRepo = followerRepo;
        _postReplicaRepository = postReplicaRepository;
        _rankingService = rankingService;
        _config = options.Value;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PostPublishedIntegrationEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Processing PostPublished event for post {PostId} by author {AuthorId}",
            message.PostId,
            message.AuthorAliasId);

        try
        {
            // Sync published post to Cassandra replica tables
            // Assuming published posts are public and finalized by default
            await _postReplicaRepository.AddPublicFinalizedPostAsync(
                message.PostId,
                message.AuthorAliasId,
                visibility: "Public",
                status: "Finalized",
                ymdBucket: DateOnly.FromDateTime(message.PublishedAt.Date),
                createdAt: null, // Will use time-ordered GUID
                ct: context.CancellationToken);

            // Add to trending if the post has good engagement potential
            await _rankingService.AddToTrendingAsync(
                message.PostId,
                score: 0.0, // Initial score, will be updated by reactions/comments
                message.PublishedAt,
                context.CancellationToken);

            _logger.LogInformation(
                "Successfully synced published post {PostId} to Cassandra replica tables",
                message.PostId);

            // Check if author is VIP (has enough followers for fan-out optimization)
            var followers = await _followerRepo.GetAllFollowersOfAliasAsync(
                message.AuthorAliasId,
                context.CancellationToken);

            var followerCount = followers.Count;

            if (followerCount < _config.VipCriteria.MinFollowers)
            {
                _logger.LogDebug(
                    "Author {AuthorId} has {Count} followers (threshold: {Threshold}), using fan-in read path",
                    message.AuthorAliasId,
                    followerCount,
                    _config.VipCriteria.MinFollowers);
                return; // Fan-in at read time
            }

            _logger.LogInformation(
                "Author {AuthorId} is VIP with {Count} followers, performing fan-out for post {PostId}",
                message.AuthorAliasId,
                followerCount,
                message.PostId);

            // Perform fan-out to all followers
            await _fanOutService.FanOutPostAsync(
                message.PostId,
                message.AuthorAliasId,
                message.PublishedAt,
                context.CancellationToken);

            _logger.LogInformation(
                "Successfully processed PostPublished event for post {PostId}",
                message.PostId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing PostPublished event for post {PostId}",
                message.PostId);
            throw;
        }
    }
}
