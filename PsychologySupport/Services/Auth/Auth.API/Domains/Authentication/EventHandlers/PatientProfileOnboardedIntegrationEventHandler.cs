using Auth.API.Domains.Authentication.ServiceContracts.Features;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Profile;
using MassTransit;

namespace Auth.API.Domains.Authentication.EventHandlers;

public class PatientProfileOnboardedIntegrationEventHandler(IUserOnboardingService service) : IConsumer<PatientProfileOnboardedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<PatientProfileOnboardedIntegrationEvent> context)
    {
        await service.MarkPatientOnboardedAsync(context.Message.UserId);
    }
}