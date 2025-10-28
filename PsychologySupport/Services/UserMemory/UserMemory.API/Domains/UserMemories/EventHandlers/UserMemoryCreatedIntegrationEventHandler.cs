using BuildingBlocks.Messaging.Events.IntegrationEvents.Chatbox;
using MassTransit;
using MediatR;
using UserMemory.API.Domains.UserMemories.Features.SaveMemory;

namespace UserMemory.API.Domains.UserMemories.EventHandlers;

public class UserMemoryCreatedIntegrationEventHandler(ISender sender) : IConsumer<UserMemoryCreatedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<UserMemoryCreatedIntegrationEvent> context)
    {
        var message = context.Message;

        await sender.Send(new SaveMemoryCommand(
            message.AliasId,
            message.SessionId,
            message.Summary,
            message.Tags,
            message.SaveNeeded
            ), context.CancellationToken);
    }
}