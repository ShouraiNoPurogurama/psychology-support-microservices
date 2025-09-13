using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace BuildingBlocks.Idempotency;

public sealed class DefaultIdempotencyHasher : IIdempotencyHasher
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public string ComputeHash(object request)
    {
        var json = JsonSerializer.Serialize(request, Options);
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(json));
        var sb = new StringBuilder(bytes.Length * 2);
        foreach (var b in bytes) sb.AppendFormat("{0:x2}", b);
        return sb.ToString();
    }
}