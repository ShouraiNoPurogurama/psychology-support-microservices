using Auth.API.Data;
using Auth.API.Features.Authentication.ServiceContracts.Shared;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Auth;
using MassTransit;

namespace Auth.API.Features.Authentication.Services.Shared;

public class TokenRevocationService(IPublishEndpoint publishEndpoint, AuthDbContext dbContext) : ITokenRevocationService
{
    //TODO Sau này scale lên nhiều node redis khác nhau thì quay lại consume event
    public async Task RevokeSessionsAsync(IEnumerable<DeviceSession> sessions)
    {
        var sessionsList = sessions.ToList();
        if (!sessionsList.Any())
        {
            return;
        }

        List<TokenRevokedIntegrationEvent> revokedEvents = [];
        
        foreach (var session in sessionsList)
        {
            session.IsRevoked = true;
            session.RevokedAt = DateTimeOffset.UtcNow;
         
            revokedEvents.Add(new TokenRevokedIntegrationEvent(session.AccessTokenId, session.RevokedAt.Value));
        }
        
        dbContext.UpdateRange(sessionsList);

        await dbContext.SaveChangesAsync();
        await publishEndpoint.PublishBatch(revokedEvents);
    }
}