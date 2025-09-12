namespace Post.Application.Integration;

public interface IOutboxWriter
{
    Task EnqueueAsync(object evt, CancellationToken ct);
}