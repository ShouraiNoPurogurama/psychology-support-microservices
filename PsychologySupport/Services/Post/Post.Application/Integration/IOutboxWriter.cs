namespace Post.Application.Integration;

public interface IOutboxWriter
{
    Task WriteAsync(object evt, CancellationToken ct);
}