using AIModeration.API.Shared.Services;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Moderation;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;
using MassTransit;

namespace AIModeration.API.Features.PostContents;

/// <summary>
/// Handles PostCreatedIntegrationEvent from Post microservice
/// Evaluates post content and publishes moderation result
/// </summary>
public class PostCreatedIntegrationEventHandler : IConsumer<PostCreatedIntegrationEvent>
{
    private readonly IContentModerationService _moderationService;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<PostCreatedIntegrationEventHandler> _logger;

    public PostCreatedIntegrationEventHandler(
        IContentModerationService moderationService,
        IPublishEndpoint publishEndpoint,
        ILogger<PostCreatedIntegrationEventHandler> logger)
    {
        _moderationService = moderationService;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PostCreatedIntegrationEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Processing post moderation for PostId: {PostId}, AuthorAliasId: {AuthorAliasId}",
            message.PostId,
            message.AuthorAliasId);

        try
        {
            // Moderate the post content
            var moderationResult = await _moderationService.ModeratePostContentAsync(
                message.PostId,
                message.Title,
                message.Content,
                context.CancellationToken);

            // Publish the moderation result event
            var resultEvent = new PostModerationEvaluatedIntegrationEvent(
                moderationResult.PostId,
                message.AuthorAliasId,   
                moderationResult.Status,
                moderationResult.Reasons,
                moderationResult.PolicyVersion,
                moderationResult.EvaluatedAt,
                message.CreatedAt   
            );

            await _publishEndpoint.Publish(resultEvent, context.CancellationToken);

            _logger.LogInformation(
                "Post moderation completed. PostId: {PostId}, Status: {Status}, Reasons: {Reasons}",
                message.PostId,
                moderationResult.Status,
                string.Join(", ", moderationResult.Reasons));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing post moderation for PostId: {PostId}",
                message.PostId);
            
            // Don't throw - we don't want to retry indefinitely for moderation failures
            // Could implement a dead letter queue or alert mechanism here
        }
    }
}
