using Auth.API.Features.Authentication.ServiceContracts.Features;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Subscription;
using MassTransit;

namespace Auth.API.Features.Authentication.EventHandlers
{
    public class UserSubscriptionExpiredIntegrationEventHandler(IUserSubscriptionService service)
        : IConsumer<UserSubscriptionExpiredIntegrationEvent>
    {
        public async Task Consume(ConsumeContext<UserSubscriptionExpiredIntegrationEvent> context)
        {
            await service.RemoveExpiredSubscriptionAsync(context.Message.PatientId);
        }
    }
}
