namespace Post.Application.Abstractions.Integration;

public interface IOutboxWriter
{
    Task WriteAsync(object evt, CancellationToken ct);
}