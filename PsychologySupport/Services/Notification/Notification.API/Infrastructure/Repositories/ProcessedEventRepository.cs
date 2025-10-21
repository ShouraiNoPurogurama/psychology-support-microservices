using Microsoft.EntityFrameworkCore;
using Notification.API.Data;
using Notification.API.Features.Notifications.Models;
using Notification.API.Infrastructure.Data;
using Notification.API.Shared.Contracts;

namespace Notification.API.Infrastructure.Repositories;

public class ProcessedEventRepository : IProcessedEventRepository
{
    private readonly NotificationDbContext _context;

    public ProcessedEventRepository(NotificationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> TryAddAsync(Guid messageId, string eventType, CancellationToken cancellationToken = default)
    {
        try
        {
            var processedEvent = ProcessedIntegrationEvent.Create(messageId, eventType);
            await _context.ProcessedIntegrationEvents.AddAsync(processedEvent, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException)
        {
            // Duplicate key - event already processed
            return false;
        }
    }

    public async Task<bool> ExistsAsync(Guid messageId, CancellationToken cancellationToken = default)
    {
        return await _context.ProcessedIntegrationEvents
            .AnyAsync(e => e.Id == messageId, cancellationToken);
    }

    public async Task<int> DeleteOlderThanAsync(DateTimeOffset cutoffDate, CancellationToken cancellationToken = default)
    {
        return await _context.ProcessedIntegrationEvents
            .Where(e => e.ReceivedAt < cutoffDate)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
