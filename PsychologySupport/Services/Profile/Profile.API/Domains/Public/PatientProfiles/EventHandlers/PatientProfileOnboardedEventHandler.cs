using BuildingBlocks.Messaging.Events.IntegrationEvents.Profile;
using Profile.API.Domains.Pii.Services;
using Profile.API.Domains.Public.PatientProfiles.Events;

namespace Profile.API.Domains.Public.PatientProfiles.EventHandlers;

public class PatientProfileOnboardedEventHandler(
    PiiService piiService,
    IPublishEndpoint publishEndpoint,
    ILogger<PatientProfileOnboardedEventHandler> logger
    ) : INotificationHandler<PatientProfileOnboardedEvent>
{
    public async Task Handle(PatientProfileOnboardedEvent notification, CancellationToken cancellationToken)
    {
        var userId = await piiService.ResolveUserIdByPatientId(notification.PatientId, cancellationToken);
        
        if (userId == Guid.Empty)
        {
            logger.LogError("No UserId found for PatientId: {PatientId}", notification.PatientId);
        }
        
        var integrationEvent = new PatientProfileOnboardedIntegrationEvent(userId);

        await publishEndpoint.Publish(integrationEvent, cancellationToken);
    }
}