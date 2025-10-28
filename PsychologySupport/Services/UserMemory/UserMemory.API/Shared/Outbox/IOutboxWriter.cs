namespace UserMemory.API.Shared.Outbox;

public interface IOutboxWriter
{
    Task WriteAsync(object evt, CancellationToken ct);
}

