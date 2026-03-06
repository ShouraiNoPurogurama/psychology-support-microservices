using System.Text.Json;
using Alias.API.Aliases.Models.OutboxMessages;
using Alias.API.Data.Public;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Alias.API.Common.Outbox;

/// <summary>
/// Background service that relays persisted outbox messages to the message broker.
/// Polls the outbox table for unprocessed messages and publishes them via MassTransit,
/// then marks them as processed. This ensures at-least-once delivery guarantees
/// for integration events published by the Alias service.
/// </summary>
public sealed class OutboxRelayService(
    IServiceScopeFactory scopeFactory,
    ILogger<OutboxRelayService> logger) : BackgroundService
{
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(5);

    // Maps fully-qualified type names stored in the outbox to the actual CLR types we can publish
    private static readonly IReadOnlyDictionary<string, Type> KnownEventTypes =
        new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
        {
            [typeof(BuildingBlocks.Messaging.Events.IntegrationEvents.Alias.AliasFollowedIntegrationEvent).FullName!]
                = typeof(BuildingBlocks.Messaging.Events.IntegrationEvents.Alias.AliasFollowedIntegrationEvent),

            [typeof(BuildingBlocks.Messaging.Events.IntegrationEvents.Alias.AliasUnfollowedIntegrationEvent).FullName!]
                = typeof(BuildingBlocks.Messaging.Events.IntegrationEvents.Alias.AliasUnfollowedIntegrationEvent),

            [typeof(BuildingBlocks.Messaging.Events.IntegrationEvents.Alias.AliasIssuedIntegrationEvent).FullName!]
                = typeof(BuildingBlocks.Messaging.Events.IntegrationEvents.Alias.AliasIssuedIntegrationEvent),

            [typeof(BuildingBlocks.Messaging.Events.IntegrationEvents.Alias.AliasUpdatedIntegrationEvent).FullName!]
                = typeof(BuildingBlocks.Messaging.Events.IntegrationEvents.Alias.AliasUpdatedIntegrationEvent),

            [typeof(BuildingBlocks.Messaging.Events.IntegrationEvents.Alias.AliasVisibilityChangedIntegrationEvent).FullName!]
                = typeof(BuildingBlocks.Messaging.Events.IntegrationEvents.Alias.AliasVisibilityChangedIntegrationEvent),
        };

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("OutboxRelayService started. Polling every {Interval}s.", PollingInterval.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Error occurred while relaying outbox messages.");
            }

            await Task.Delay(PollingInterval, stoppingToken);
        }
    }

    private async Task ProcessPendingOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AliasDbContext>();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        // Fetch a batch of unprocessed messages ordered by occurrence time
        var pending = await db.OutboxMessages
            .Where(m => m.ProcessedOn == null)
            .OrderBy(m => m.OccurredOn)
            .Take(50)
            .ToListAsync(cancellationToken);

        if (pending.Count == 0) return;

        logger.LogInformation("OutboxRelayService: relaying {Count} pending outbox message(s).", pending.Count);

        foreach (var message in pending)
        {
            try
            {
                if (!KnownEventTypes.TryGetValue(message.Type, out var eventType))
                {
                    logger.LogWarning("OutboxRelayService: unknown event type '{Type}', skipping.", message.Type);
                    message.ProcessedOn = DateTimeOffset.UtcNow;
                    continue;
                }

                var evt = JsonSerializer.Deserialize(message.Content, eventType);
                if (evt is null)
                {
                    logger.LogWarning("OutboxRelayService: failed to deserialize message {Id} of type '{Type}'.", message.Id, message.Type);
                    message.ProcessedOn = DateTimeOffset.UtcNow;
                    continue;
                }

                await publishEndpoint.Publish(evt, eventType, cancellationToken);
                message.ProcessedOn = DateTimeOffset.UtcNow;

                logger.LogInformation("OutboxRelayService: published message {Id} of type '{Type}'.", message.Id, message.Type);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "OutboxRelayService: failed to publish message {Id} of type '{Type}'.", message.Id, message.Type);
                // Do not mark as processed — will be retried on next poll
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
