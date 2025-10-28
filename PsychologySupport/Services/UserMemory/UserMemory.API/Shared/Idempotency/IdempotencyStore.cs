namespace UserMemory.API.Shared.Idempotency;

public class IdempotencyStore
{
    private readonly Dictionary<string, (int Status, string Body)> _cache = new();

    public bool TryGet(string key, out (int, string) response) => _cache.TryGetValue(key, out response);
    public void Set(string key, int status, string body) => _cache[key] = (status, body);
}