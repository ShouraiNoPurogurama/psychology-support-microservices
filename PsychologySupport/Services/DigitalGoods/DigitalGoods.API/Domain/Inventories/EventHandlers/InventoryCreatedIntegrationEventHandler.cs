using BuildingBlocks.Messaging.Events.IntegrationEvents.Subscription;
using DigitalGoods.API.Domain.Inventories.Features.CreateInventory;
using MassTransit;
using MediatR;

namespace DigitalGoods.API.Domain.Inventories.EventHandlers
{
    public class InventoryCreatedIntegrationEventHandler(ISender sender) : IConsumer<InventoryCreatedIntegrationEvent>
    {
        public async Task Consume(ConsumeContext<InventoryCreatedIntegrationEvent> context)
        {
            await sender.Send(new CreateInventoryCommand(context.Message.SubjectRef, context.Message.SubscriptionStartDate,context.Message.SubscriptionEndDate));
        }
    }
}
