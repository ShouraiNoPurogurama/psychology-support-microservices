using System.Security.Cryptography;
using System.Text;

namespace Alias.API.Common.Security;

public class AliasTokenService(IConfiguration config) : IAliasTokenService
{
    private readonly byte[] _secret = Encoding.UTF8.GetBytes(config["Alias:TokenSecret"]!);

    public string Create(string aliasKey, DateTimeOffset expiresAt)
    {
        var payload = $"{aliasKey}|{expiresAt.UtcDateTime:O}";
        var signature = ComputeHmac(payload);
        var token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{payload}|{signature}"));
        return token;
    }

    public bool TryValidate(string token, out string aliasKey, out DateTimeOffset expiresAt)
    {
        aliasKey = string.Empty;
        expiresAt = default;

        string raw;
        try
        {
            raw = Encoding.UTF8.GetString(Convert.FromBase64String(token));
        }
        catch
        {
            return false;
        }

        var parts = raw.Split('|');
        if(parts.Length != 3) return false;
        
        aliasKey = parts[0];
        if(!DateTimeOffset.TryParse(parts[1], out expiresAt)) return false;
        var signature = parts[2];
        
        var payload = $"{aliasKey}|{expiresAt.UtcDateTime:O}";
        var expect = ComputeHmac(payload);
        if (!CryptographicOperations.FixedTimeEquals(Convert.FromHexString(signature), Convert.FromHexString(expect)))
            return false;
        
        return expiresAt > DateTimeOffset.UtcNow;
    }

    private string ComputeHmac(string payload)
    {
        using var h = new HMACSHA256(_secret);
        var hash = h.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return Convert.ToBase64String(hash);
    }
}