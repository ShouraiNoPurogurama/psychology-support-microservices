using BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;
using Feed.Application.Abstractions.FanOut;
using Feed.Application.Abstractions.FollowerTracking;
using Feed.Application.Configuration;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Feed.Application.Features.Consumers.Post;

/// <summary>
/// Consumes PostPublishedIntegrationEvent and fans out the post to all followers.
/// Only performs fan-out if the author is a VIP (has enough followers).
/// </summary>
public sealed class PostPublishedIntegrationEventConsumer : IConsumer<PostPublishedIntegrationEvent>
{
    private readonly IFeedFanOutService _fanOutService;
    private readonly IFollowerTrackingRepository _followerRepo;
    private readonly FeedConfiguration _config;
    private readonly ILogger<PostPublishedIntegrationEventConsumer> _logger;

    public PostPublishedIntegrationEventConsumer(
        IFeedFanOutService fanOutService,
        IFollowerTrackingRepository followerRepo,
        IOptions<FeedConfiguration> options,
        ILogger<PostPublishedIntegrationEventConsumer> logger)
    {
        _fanOutService = fanOutService;
        _followerRepo = followerRepo;
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
