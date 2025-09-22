namespace Feed.Infrastructure.Resilience.Rules;

internal static class IdempotencyRedisKeys
{
    public static string Exists(Guid key) => $"idem:exists:{key:N}";
    public static string Resp(Guid key)   => $"idem:resp:{key:N}";
    public static string Lock(Guid key)   => $"idem:lock:{key:N}";
}
