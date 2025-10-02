using BuildingBlocks.Messaging.Events.IntegrationEvents.Alias;
using Feed.Application.Abstractions.ViewerBlocking;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Feed.Application.Features.Consumers.Alias;

/// <summary>
/// Consumes UserUnblockedIntegrationEvent and removes the blocking relationship.
/// When a user unblocks another user, posts from the previously blocked user
/// can now appear in the unblocker's feed.
/// </summary>
public sealed class UserUnblockedIntegrationEventConsumer : IConsumer<UserUnblockedIntegrationEvent>
{
    private readonly IViewerBlockingRepository _blockingRepo;
    private readonly ILogger<UserUnblockedIntegrationEventConsumer> _logger;

    public UserUnblockedIntegrationEventConsumer(
        IViewerBlockingRepository blockingRepo,
        ILogger<UserUnblockedIntegrationEventConsumer> logger)
    {
        _blockingRepo = blockingRepo;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserUnblockedIntegrationEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Processing UserUnblocked event: {Unblocker} unblocked {Unblocked}",
            message.UnblockerAliasId,
            message.UnblockedAliasId);

        try
        {
            // Remove blocking relationship
            await _blockingRepo.RemoveAsync(
                message.UnblockerAliasId,
                message.UnblockedAliasId,
                context.CancellationToken);

            _logger.LogInformation(
                "Successfully processed UserUnblocked event: {Unblocker} -> {Unblocked}",
                message.UnblockerAliasId,
                message.UnblockedAliasId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing UserUnblocked event: {Unblocker} -> {Unblocked}",
                message.UnblockerAliasId,
                message.UnblockedAliasId);
            throw;
        }
    }
}
