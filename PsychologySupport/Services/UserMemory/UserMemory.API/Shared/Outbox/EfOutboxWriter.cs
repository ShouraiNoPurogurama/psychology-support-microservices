using System.Text.Json;
using UserMemory.API.Data;
using UserMemory.API.Models;

namespace UserMemory.API.Shared.Outbox;

public sealed class EfOutboxWriter : IOutboxWriter
{
    private readonly UserMemoryDbContext _db;
    public EfOutboxWriter(UserMemoryDbContext db) => _db = db;

    public Task WriteAsync(object evt, CancellationToken ct)
    {
        _db.Set<OutboxMessage>().Add(new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = evt.GetType().FullName!,
            Content = JsonSerializer.Serialize(evt),
            OccurredOn = DateTimeOffset.UtcNow
        });
        return Task.CompletedTask;
    }
}
