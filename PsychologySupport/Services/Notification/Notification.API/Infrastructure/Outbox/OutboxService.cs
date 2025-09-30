using BuildingBlocks.Messaging.Events.IntegrationEvents;
using Notification.API.Infrastructure.Data;
using Notification.API.Infrastructure.Persistence.Models;

namespace Notification.API.Infrastructure.Outbox;

public class OutboxService(NotificationDbContext dbContext)
{
    public async Task AddAsync(OutboxMessage outboxMessage, CancellationToken cancellationToken)
    {
        await dbContext.OutboxMessages.AddAsync(outboxMessage, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> HasMessageAsync<TMessage>(TMessage message, CancellationToken cancellationToken)
        where TMessage : IntegrationEvent
    {
        return await dbContext.OutboxMessages.AnyAsync(x => x.Id.Equals(message.Id), cancellationToken);
    }

    public async Task<List<OutboxMessage>> GetUnprocessedMessagesAsync(CancellationToken cancellationToken)
    {
        return await dbContext.OutboxMessages
            .Where(x => x.ProcessedOn == null)
            .OrderBy(x => x.OccuredOn)
            .ToListAsync(cancellationToken);
    }

    public async Task ProcessMessageAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        message.Process();
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}