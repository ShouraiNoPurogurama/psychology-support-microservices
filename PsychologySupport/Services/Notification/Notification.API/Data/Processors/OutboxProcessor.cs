using System.Text.Json;

namespace Notification.API.Data.Processors;

public class OutboxProcessor(IServiceProvider serviceProvider, ILogger<OutboxProcessor> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
                var outboxMessages = await dbContext.OutboxMessages
                    .Where(o => o.ProcessedOn == null)
                    .ToListAsync(stoppingToken);

                foreach (var message in outboxMessages)
                {
                    var eventType = Type.GetType(message.Type);
                    if (eventType is null)
                    {
                        logger.LogWarning("Could not resolve type: {Type}", message.Type);
                        continue;
                    }

                    var eventMessage = JsonSerializer.Deserialize(message.Content, eventType);
                    if (eventMessage is null)
                    {
                        logger.LogWarning("Could not deserialize message: {Content}", message.Content);
                        continue;
                    }

                    if (eventMessage is IDomainEvent)
                    {
                        await mediator.Publish(eventMessage, stoppingToken);
                    }

                    message.ProcessedOn = DateTimeOffset.UtcNow;

                    logger.LogInformation("Successfully processed outbox message with ID: {ID}", message.Id);
                    await dbContext.SaveChangesAsync(stoppingToken);
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error processing outbox message");
            }

            //Delay: Just check for Outbox table for each 3 seconds
            await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
        }
    }
}