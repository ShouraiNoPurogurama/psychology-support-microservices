using BuildingBlocks.Messaging.Events.IntegrationEvents.Media;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Post.Application.Features.Posts.Commands.FinalizePostCreation;

namespace Post.Application.Features.Posts.EventHandlers;

public class MediaAttachedToOwnerIntegrationEventHandler(
    ISender sender,
    ILogger<MediaAttachedToOwnerIntegrationEventHandler> logger)
    : IConsumer<MediaAttachedToOwnerIntegrationEvent>
{
    public async Task Consume(ConsumeContext<MediaAttachedToOwnerIntegrationEvent> context)
    {
        var message = context.Message;

        if (message.OwnerType != nameof(Post))
        {
            return;
        }

        //Logic `AttachMedia` nên được xử lý ở một consumer khác trước khi gọi Finalize.
        //Tuy nhiên, để hoàn tất Saga, bước cuối cùng là gọi FinalizeCreation.

        logger.LogInformation(
            "Received media attachment success for PostId: {PostId}. Finalizing creation saga step.",
            message.OwnerId);

        var command = new FinalizePostCreationCommand(message.OwnerId);

        await sender.Send(command, context.CancellationToken);
    }
}