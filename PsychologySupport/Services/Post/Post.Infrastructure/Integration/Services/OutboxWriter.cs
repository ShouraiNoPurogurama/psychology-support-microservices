using System.Text.Json;
using Post.Application.Integration;
using Post.Infrastructure.Data.Post;
using Post.Infrastructure.Integration.Entities;

namespace Post.Infrastructure.Integration.Services;

public sealed class EfOutboxWriter : IOutboxWriter
{
    private readonly PostDbContext _db;
    public EfOutboxWriter(PostDbContext db) => _db = db;

    public Task EnqueueAsync(object evt, CancellationToken ct)
    {
        _db.OutboxMessages.Add(new OutboxMessage {
            Id = Guid.NewGuid(),
            Type = evt.GetType().FullName!,
            Content = JsonSerializer.Serialize(evt),
            OccurredOn = DateTime.UtcNow
        });
        return Task.CompletedTask;
    }
}