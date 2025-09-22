using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Feed.Application.Abstractions.CursorService;
using Microsoft.Extensions.Configuration;

namespace Feed.Infrastructure.Data.Redis;

public sealed class CursorService : ICursorService
{
    private readonly string _secret;

    public CursorService(IConfiguration configuration)
    {
        _secret = configuration["Feed:CursorSecret"] ?? "default-secret-key";
    }

    public string EncodeCursor(int offset, DateTime snapshotTs)
    {
        var data = new CursorData(offset, snapshotTs.ToUniversalTime());
        var json = JsonSerializer.Serialize(data);
        var hmac = GenerateHmac(json);
        var cursorWithHmac = new CursorWithHmac(json, hmac);
        var finalJson = JsonSerializer.Serialize(cursorWithHmac);
        
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(finalJson));
    }

    public (int Offset, DateTime SnapshotTs) DecodeCursor(string cursor)
    {
        try
        {
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
            var cursorWithHmac = JsonSerializer.Deserialize<CursorWithHmac>(json)
                ?? throw new ArgumentException("Invalid cursor format");

            // Validate HMAC
            var expectedHmac = GenerateHmac(cursorWithHmac.Data);
            if (cursorWithHmac.Hmac != expectedHmac)
                throw new ArgumentException("Invalid cursor signature");

            var cursorData = JsonSerializer.Deserialize<CursorData>(cursorWithHmac.Data)
                ?? throw new ArgumentException("Invalid cursor data");

            return (cursorData.Offset, cursorData.SnapshotTs);
        }
        catch (Exception ex) when (ex is JsonException or FormatException or ArgumentException)
        {
            throw new ArgumentException("Invalid cursor format", ex);
        }
    }

    public bool ValidateCursor(string cursor)
    {
        try
        {
            DecodeCursor(cursor);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private string GenerateHmac(string data)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToBase64String(hash);
    }
}

internal record CursorData(int Offset, DateTime SnapshotTs);
internal record CursorWithHmac(string Data, string Hmac);
