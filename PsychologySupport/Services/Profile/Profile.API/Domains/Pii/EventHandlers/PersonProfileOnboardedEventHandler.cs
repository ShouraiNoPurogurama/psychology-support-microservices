using BuildingBlocks.Messaging.Events.IntegrationEvents.Pii;
using Profile.API.Domains.Pii.Events;
using Profile.API.Domains.Pii.Features.GetUserIdFromSubjectRef;

namespace Profile.API.Domains.Pii.EventHandlers;

public class PersonProfileOnboardedEventHandler(ISender sender, IPublishEndpoint publishEndpoint, ILogger<PersonProfileOnboardedEventHandler> logger) : INotificationHandler<PersonProfileOnboardedEvent>
{
    public async Task Handle(PersonProfileOnboardedEvent notification, CancellationToken cancellationToken)
    {
        var command = new GetUserIdFromSubjectRefCommand(notification.SubjectRef);

        var result = await sender.Send(command, cancellationToken);

        var userId = result.UserId;
        
        if (userId == Guid.Empty)
        {
            logger.LogError("No UserId found for SubjectRef: {SubjectRef}", notification.SubjectRef);
        }

        var integrationEvent = new PersonProfileOnboardedIntegrationEvent(userId);

        await publishEndpoint.Publish(integrationEvent, cancellationToken);
    }
}