using Auth.API.Features.Authentication.ServiceContracts.Shared;
using BuildingBlocks.Constants;
using StackExchange.Redis;

namespace Auth.API.Features.Authentication.Services.Shared;

public class CachedTokenRevocationService(
    ITokenRevocationService decoratedService,
    IConnectionMultiplexer redisConnection
) : ITokenRevocationService
{

    public async Task RevokeSessionsAsync(IEnumerable<DeviceSession> sessions)
    {
        var sessionsList = sessions.ToList();
        if (!sessionsList.Any())
        {
            await decoratedService.RevokeSessionsAsync(sessionsList);
            return;
        }

        var redisDb = redisConnection.GetDatabase();
        var redisTasks = new List<Task>();

        var fixedTtl = TimeSpan.FromHours(1.5);

        foreach (var session in sessionsList)
        {
            var jti = session.AccessTokenId;
            var redisKey = MyStrings.GenerateRevokedTokenKey(jti);
            
            var redisValue = "1";

            redisTasks.Add(redisDb.StringSetAsync(redisKey, redisValue, fixedTtl));
        }
        
        if (redisTasks.Any())
        {
            await Task.WhenAll(redisTasks);
        }
        
        await decoratedService.RevokeSessionsAsync(sessionsList);
    }
}