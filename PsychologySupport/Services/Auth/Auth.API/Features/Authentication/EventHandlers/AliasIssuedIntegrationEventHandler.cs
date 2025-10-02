using Auth.API.Features.Authentication.ServiceContracts.Features;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Alias;
using MassTransit;

namespace Auth.API.Features.Authentication.EventHandlers;

public class AliasIssuedIntegrationEventHandler(IUserOnboardingService service) : IConsumer<AliasIssuedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<AliasIssuedIntegrationEvent> context)
    {
        await service.MarkMarkAliasIssuedAsync(context.Message.UserId);
    }
}