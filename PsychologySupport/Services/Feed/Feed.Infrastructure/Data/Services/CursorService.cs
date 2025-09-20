using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Feed.Application.Abstractions.CursorService;

namespace Feed.Infrastructure.Data.Services;

public sealed class CursorService : ICursorService
{
    private readonly string _secretKey = "feed-cursor-secret-key-2024"; // In production, use IConfiguration
    
    public string EncodeCursor(int pageIndex, DateTime snapshotTime)
    {
        var data = new CursorData(pageIndex, snapshotTime);
        var json = JsonSerializer.Serialize(data);
        var bytes = Encoding.UTF8.GetBytes(json);
        
        // Add HMAC for integrity
        var hmac = ComputeHmac(bytes);
        var signedData = new SignedCursorData(Convert.ToBase64String(bytes), hmac);
        
        var signedJson = JsonSerializer.Serialize(signedData);
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(signedJson));
    }
    
    public (int PageIndex, DateTime SnapshotTime) DecodeCursor(string cursor)
    {
        try
        {
            var signedBytes = Convert.FromBase64String(cursor);
            var signedJson = Encoding.UTF8.GetString(signedBytes);
            var signedData = JsonSerializer.Deserialize<SignedCursorData>(signedJson);
            
            if (signedData == null)
                throw new ArgumentException("Invalid cursor format");
            
            var dataBytes = Convert.FromBase64String(signedData.Data);
            var expectedHmac = ComputeHmac(dataBytes);
            
            if (signedData.Hmac != expectedHmac)
                throw new ArgumentException("Cursor integrity check failed");
            
            var json = Encoding.UTF8.GetString(dataBytes);
            var data = JsonSerializer.Deserialize<CursorData>(json);
            
            return data == null 
                ? (0, DateTime.UtcNow) 
                : (data.PageIndex, data.SnapshotTime);
        }
        catch
        {
            // Return safe defaults on any parsing error
            return (0, DateTime.UtcNow);
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
    
    private string ComputeHmac(byte[] data)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_secretKey));
        var hash = hmac.ComputeHash(data);
        return Convert.ToBase64String(hash);
    }
    
    private record CursorData(int PageIndex, DateTime SnapshotTime);
    private record SignedCursorData(string Data, string Hmac);
}
