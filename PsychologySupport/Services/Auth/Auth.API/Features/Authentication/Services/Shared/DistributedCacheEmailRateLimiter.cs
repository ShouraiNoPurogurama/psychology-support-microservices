using Auth.API.Features.Authentication.ServiceContracts.Shared;
using Microsoft.Extensions.Caching.Distributed;

namespace Auth.API.Features.Authentication.Services.Shared;

public class DistributedCacheEmailRateLimiter(IDistributedCache cache) : IEmailRateLimiter
{
    private const int RateLimitSeconds = 65;

    private string GetCacheKey(string email) => $"rate-limit:email:{email}";

    public async Task<bool> HasSentRecentlyAsync(string email)
    {
        try
        {
            var key = GetCacheKey(email);
            var value = await cache.GetStringAsync(key);
            return value is not null;
        }
        catch (Exception) 
        {
            //Fail Open: Nếu cache lỗi, coi như chưa gửi gần đây.
            return false;
        }
    }

    public async Task MarkAsSentAsync(string email)
    {
        try
        {
            var key = GetCacheKey(email);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(RateLimitSeconds)
            };
            await cache.SetStringAsync(key, "sent", options);
        }
        catch (Exception)
        {
            // Nếu không ghi được vào cache cũng không sao.
            // Lần gửi sau có thể sẽ bị trùng, nhưng chức năng chính vẫn hoạt động.
        }
    }
}