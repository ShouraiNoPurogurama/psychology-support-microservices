using BuildingBlocks.Messaging.Events;
using Notification.API.Data;
using Notification.API.Domains.Outbox.Models;

namespace Notification.API.Domains.Outbox.Services;

public class OutboxService(NotificationDbContext dbContext)
{
    public async Task AddAsync(OutboxMessage outboxMessage, CancellationToken cancellationToken)
    {
        await dbContext.OutboxMessages.AddAsync(outboxMessage, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> HasMessageAsync<TMessage>(TMessage message, CancellationToken cancellationToken)
        where TMessage : IntegrationEvents
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