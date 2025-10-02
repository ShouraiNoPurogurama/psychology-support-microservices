using Auth.API.Features.Authentication.ServiceContracts.Features;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Profile;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Subscription;
using MassTransit;

namespace Auth.API.Features.Authentication.EventHandlers
{
    public class UserSubscriptionActivatedIntegrationEventHandler(IUserSubscriptionService service) : IConsumer<UserSubscriptionActivatedIntegrationEvent>
    {
        public async Task Consume(ConsumeContext<UserSubscriptionActivatedIntegrationEvent> context)
        {
            await service.UpdateSubscriptionPlanNameAsync(context.Message.SubjectRef,context.Message.PlanName);
        }
    }
}
