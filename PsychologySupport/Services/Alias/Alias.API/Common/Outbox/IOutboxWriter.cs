namespace Alias.API.Common.Outbox;

public interface IOutboxWriter
{
    Task WriteAsync(object evt, CancellationToken ct);
}

