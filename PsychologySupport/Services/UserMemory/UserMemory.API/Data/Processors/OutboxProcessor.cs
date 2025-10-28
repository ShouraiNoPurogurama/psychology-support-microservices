using System.Text.Json;
using BuildingBlocks.Messaging.Events.IntegrationEvents.UserMemory;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace UserMemory.API.Data.Processors;

public sealed class OutboxProcessor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxProcessor> _logger;
    private readonly TimeSpan _processingInterval = TimeSpan.FromSeconds(3);

    public OutboxProcessor(
        IServiceProvider serviceProvider,
        ILogger<OutboxProcessor> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }
    
    private static readonly HashSet<string> ExclusiveTypes = [typeof(RewardRequestedIntegrationEvent).FullName!];


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OutboxProcessor started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing outbox messages");
            }

            await Task.Delay(_processingInterval, stoppingToken);
        }

        _logger.LogInformation("OutboxProcessor stopped");
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<UserMemoryDbContext>();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        // Fetch unprocessed messages
        var messages = await dbContext.OutboxMessages
            .Where(m => m.ProcessedOn == null && 
                        !ExclusiveTypes.Contains(m.Type))
            .OrderBy(m => m.OccurredOn)
            .Take(100)
            .ToListAsync(cancellationToken);

        if (messages.Count == 0)
        {
            return;
        }

        _logger.LogInformation("Processing {Count} outbox messages", messages.Count);

        foreach (var message in messages)
        {
            try
            {
                // Deserialize the event type
                // var eventType = Type.GetType(message.Type);
                var eventType = FindType(message.Type);
                if (eventType == null)
                {
                    _logger.LogWarning("Could not resolve event type: {Type} for message {MessageId}", 
                        message.Type, message.Id);
                    
                    // Mark as processed to avoid retrying forever
                    message.ProcessedOn = DateTimeOffset.UtcNow;
                    continue;
                }

                // Deserialize the event content
                var eventMessage = JsonSerializer.Deserialize(message.Content, eventType);
                if (eventMessage == null)
                {
                    _logger.LogWarning("Could not deserialize message content for message {MessageId}", 
                        message.Id);
                    
                    // Mark as processed to avoid retrying forever
                    message.ProcessedOn = DateTimeOffset.UtcNow;
                    continue;
                }

                // Publish the event to the message broker
                await publishEndpoint.Publish(eventMessage, cancellationToken);

                // Mark as processed
                message.ProcessedOn = DateTimeOffset.UtcNow;

                _logger.LogDebug("Successfully published outbox message {MessageId} of type {Type}", 
                    message.Id, message.Type);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Error processing outbox message {MessageId} of type {Type}", 
                    message.Id, message.Type);
                
                // Don't mark as processed so it can be retried
                // Consider adding retry count and dead letter handling in production
            }
        }

        // Save all changes in a single transaction
        await dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Completed processing {Count} outbox messages", messages.Count);
    }
    
    private static Type? FindType(string typeName)
    {
        // Thử tìm trong assembly hiện tại trước cho nhanh
        var type = Type.GetType(typeName);
        if (type != null)
        {
            return type;
        }

        // Nếu không thấy, duyệt qua tất cả các assembly đã được load trong AppDomain
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            type = assembly.GetType(typeName);
            if (type != null)
            {
                return type;
            }
        }

        return null;
    }
}