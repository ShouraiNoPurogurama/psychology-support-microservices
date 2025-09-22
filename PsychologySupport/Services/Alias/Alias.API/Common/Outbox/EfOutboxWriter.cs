using System.Text.Json;
using Alias.API.Data.Public;

namespace Alias.API.Common.Outbox;

public sealed class EfOutboxWriter : IOutboxWriter
{
    private readonly AliasDbContext _db;
    public EfOutboxWriter(AliasDbContext db) => _db = db;

    public Task WriteAsync(object evt, CancellationToken ct)
    {
        _db.Set<OutboxMessage>().Add(new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = evt.GetType().FullName!,
            Content = JsonSerializer.Serialize(evt),
            OccurredOn = DateTime.UtcNow
        });
        return Task.CompletedTask;
    }
}
