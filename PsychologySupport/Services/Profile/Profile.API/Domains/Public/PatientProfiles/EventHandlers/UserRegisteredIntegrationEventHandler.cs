using BuildingBlocks.Messaging.Events.IntegrationEvents.Auth;

namespace Profile.API.Domains.PatientProfiles.EventHandlers;

public class UserRegisteredIntegrationEventHandler : IConsumer<UserRegisteredIntegrationEvent>
{
    public Task Consume(ConsumeContext<UserRegisteredIntegrationEvent> context)
    {
        throw new NotImplementedException();
    }
}