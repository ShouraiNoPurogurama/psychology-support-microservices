using BuildingBlocks.Messaging.Events.IntegrationEvents.Moderation; 
using Feed.Application.Abstractions.PostRepository;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Feed.Application.Features.Consumers.Post;

public sealed class PostModerationEvaluatedIntegrationEventConsumer 
    : IConsumer<PostModerationEvaluatedIntegrationEvent> 
{
    private readonly IPostReplicaRepository _postReplicaRepository;
    private readonly ILogger<PostModerationEvaluatedIntegrationEventConsumer> _logger;

    public PostModerationEvaluatedIntegrationEventConsumer(
        IPostReplicaRepository postReplicaRepository,
        ILogger<PostModerationEvaluatedIntegrationEventConsumer> logger)
    {
        _postReplicaRepository = postReplicaRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PostModerationEvaluatedIntegrationEvent> context)
    {
        var message = context.Message;

        // CHỈ xử lý nếu post được duyệt
        if (message.Status != "Approved") 
        {
            _logger.LogInformation(
                "Skipping post {PostId} for feed, status: {Status}",
                message.PostId,
                message.Status);
            return;
        }

        _logger.LogInformation(
            "Processing Approved post {PostId} by AI moderation",
            message.PostId);

        try
        {
            await _postReplicaRepository.AddPublicFinalizedPostAsync(
                message.PostId,
                message.AuthorAliasId,
                visibility: "Public",
                status: "Approved",
                ymdBucket: DateOnly.FromDateTime(message.EvaluatedAt.Date),
                createdAt: message.PostCreatedAt, 
                ct: context.CancellationToken);

            _logger.LogInformation(
                "Successfully added approved post {PostId} to Cassandra replica tables",
                message.PostId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing Approved (by AI) event for post {PostId}",
                message.PostId);
            throw;
        }
    }
}