using BuildingBlocks.Messaging.Events.IntegrationEvents.Alias;
using Feed.Application.Abstractions.ViewerBlocking;
using Feed.Domain.ViewerBlocking;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Feed.Application.Features.Consumers.Alias;

/// <summary>
/// Consumes UserBlockedIntegrationEvent and updates the viewer blocking table.
/// When a user blocks another user, posts from the blocked user should not appear
/// in the blocker's feed.
/// </summary>
public sealed class UserBlockedIntegrationEventConsumer : IConsumer<UserBlockedIntegrationEvent>
{
    private readonly IViewerBlockingRepository _blockingRepo;
    private readonly ILogger<UserBlockedIntegrationEventConsumer> _logger;

    public UserBlockedIntegrationEventConsumer(
        IViewerBlockingRepository blockingRepo,
        ILogger<UserBlockedIntegrationEventConsumer> logger)
    {
        _blockingRepo = blockingRepo;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserBlockedIntegrationEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Processing UserBlocked event: {Blocker} blocked {Blocked}",
            message.BlockerAliasId,
            message.BlockedAliasId);

        try
        {
            // Create ViewerBlocked entity
            var viewerBlocked = ViewerBlocked.Create(
                message.BlockerAliasId,
                message.BlockedAliasId,
                DateTimeOffset.UtcNow);

            // Add to blocking table (if not exists)
            await _blockingRepo.AddIfNotExistsAsync(viewerBlocked, context.CancellationToken);

            _logger.LogInformation(
                "Successfully processed UserBlocked event: {Blocker} -> {Blocked}",
                message.BlockerAliasId,
                message.BlockedAliasId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing UserBlocked event: {Blocker} -> {Blocked}",
                message.BlockerAliasId,
                message.BlockedAliasId);
            throw;
        }
    }
}
