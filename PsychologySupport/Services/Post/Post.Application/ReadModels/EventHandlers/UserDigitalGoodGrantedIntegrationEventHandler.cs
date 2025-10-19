using BuildingBlocks.Messaging.Events.IntegrationEvents.DigitalGood;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Post.Application.ReadModels.Commands.CreateUserOwnedGiftReplica;
using Post.Application.ReadModels.Commands.CreateUserOwnedTagReplica;

namespace Post.Application.ReadModels.EventHandlers;

public class UserDigitalGoodGrantedIntegrationEventHandler(
    ISender sender,
    ILogger<UserDigitalGoodGrantedIntegrationEventHandler> logger)
    : IConsumer<UserDigitalGoodGrantedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<UserDigitalGoodGrantedIntegrationEvent> context)
    {
        var message = context.Message;

        if (message.AliasId == Guid.Empty)
        {
            logger.LogError("Invalid AliasId in UserDigitalGoodGrantedIntegrationEvent.");
            return;
        }

        // UserOwnedTagReplica
        var createTagCommand = new CreateUserOwnedTagReplicaCommand(
            message.AliasId,
            message.ValidFrom,
            message.ValidTo
        );

        var tagResult = await sender.Send(createTagCommand, context.CancellationToken);
        if (!tagResult.IsSuccess)
        {
            logger.LogError(
                "Failed to create/update UserOwnedTagReplica for AliasId: {AliasId}",
                message.AliasId);
        }

        // UserOwnedGiftReplica
        var createGiftCommand = new CreateUserOwnedGiftReplicaCommand(
            message.AliasId,
            message.ValidFrom,
            message.ValidTo
        );

        var giftResult = await sender.Send(createGiftCommand, context.CancellationToken);
        if (!giftResult.IsSuccess)
        {
            logger.LogError(
                "Failed to create/update UserOwnedGiftReplica for AliasId: {AliasId}",
                message.AliasId);
        }

        logger.LogInformation(
            "Handled UserDigitalGoodGrantedIntegrationEvent for AliasId: {AliasId}",
            message.AliasId);
    }
}
