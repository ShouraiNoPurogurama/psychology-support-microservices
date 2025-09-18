using BuildingBlocks.Messaging.Events.IntegrationEvents.Alias;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Post.Application.ReadModels.Commands.CreateAliasVersionReplica;

namespace Post.Application.ReadModels.EventHandlers;

public class AliasIssuedIntegrationEventHandler(ISender sender, ILogger<AliasIssuedIntegrationEventHandler> logger) : IConsumer<AliasIssuedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<AliasIssuedIntegrationEvent> context)
    {
        var command = new CreateAliasVersionReplicaCommand(
            context.Message.AliasId,
            context.Message.SubjectRef,
            context.Message.AliasVersionId,
            context.Message.Label,
            context.Message.ValidFrom
        );
        
        var result = await sender.Send(command, context.CancellationToken);

        if (!result.IsSuccess)
        {
            logger.LogError("Failed to create Alias Version Replica for AliasId: {AliasId}", context.Message.AliasId);
        }
    }
}