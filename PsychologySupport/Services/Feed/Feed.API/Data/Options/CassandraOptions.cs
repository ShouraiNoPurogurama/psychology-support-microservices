namespace Feed.API.Data.Options;

public sealed class CassandraOptions
{
    public string[] ContactPoints { get; init; } = Array.Empty<string>();
    public int Port { get; init; } = 9042;
    public string Keyspace { get; init; } = default!;
    public string LocalDc { get; init; } = "dc1";
    public string? Username { get; init; }
    public string? Password { get; init; }
}