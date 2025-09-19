// Media.Application/Features/Media/EventHandlers/PostCreatedWithMediaPendingIntegrationEventHandler.cs
using BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;
using MassTransit;
using Media.Application.Features.Media.Commands.AttachMediaToOwner;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Media.Application.Features.Media.EventHandlers;

public class PostCreatedWithMediaPendingIntegrationEventHandler(
    ISender sender,
    ILogger<PostCreatedWithMediaPendingIntegrationEventHandler> logger)
    : IConsumer<PostCreatedWithMediaPendingIntegrationEvent>
{
    public async Task Consume(ConsumeContext<PostCreatedWithMediaPendingIntegrationEvent> context)
    {
        var message = context.Message;
        logger.LogInformation(
            "Received PostCreated event with {MediaCount} media items. PostId: {PostId}",
            message.MediaIds.Count(), message.PostId);

        if ( !message.MediaIds.Any())
        {
            return;
        }

        var attachmentTasks = message.MediaIds.Select(mediaId =>
        {
            var command = new AttachMediaToOwnerCommand(
                OwnerId: message.PostId,
                OwnerType: "Post", //Consumer này chỉ dành riêng cho event tạo Post
                MediaId: mediaId
            );
            return sender.Send(command, context.CancellationToken);
        });

        //Chờ cho tất cả các task xử lý hoàn tất.
        await Task.WhenAll(attachmentTasks);

        logger.LogInformation(
            "Finished processing media attachments for PostId: {PostId}", message.PostId);
    }
}