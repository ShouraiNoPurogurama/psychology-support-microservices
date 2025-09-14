using BuildingBlocks.Messaging.Events.IntegrationEvents.Alias;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Post.Application.ReadModels.Commands.UpdateAliasVersionReplica;

namespace Post.Application.ReadModels.EventHandlers;

public class AliasUpdatedIntegrationEventHandler(ISender sender, ILogger<AliasUpdatedIntegrationEventHandler> logger)
    : IConsumer<AliasUpdatedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<AliasUpdatedIntegrationEvent> context)
    {
        var command = new UpdateAliasVersionReplicaCommand(
            context.Message.AliasId,
            context.Message.SubjectRef,
            context.Message.AliasVersionId,
            context.Message.Label,
            context.Message.ValidFrom
        );
        
        var result = await sender.Send(command, context.CancellationToken);

        if (!result.IsSuccess)
        {
            logger.LogError("Failed to update Alias Version Replica for AliasId: {AliasId}", context.Message.AliasId);
        }
    }
}