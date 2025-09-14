using Auth.API.Domains.Authentication.ServiceContracts.Features;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Pii;
using MassTransit;

namespace Auth.API.Domains.Authentication.EventHandlers;

public class PersonProfileOnboardedIntegrationEventHandler(IUserOnboardingService service) : IConsumer<PersonProfileOnboardedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<PersonProfileOnboardedIntegrationEvent> context)
    {
        await service.MarkPiiOnboardedAsync(context.Message.UserId);
    }
}