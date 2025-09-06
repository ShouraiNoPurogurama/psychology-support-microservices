using BuildingBlocks.Messaging.Events.IntegrationEvents.Alias;
using FluentValidation;
using Profile.API.Domains.Pii.Features.MapAliasOwner;

namespace Profile.API.Domains.Pii.EventHandlers;

public class AliasIssuedIntegrationEventHandler(ISender sender, ILogger<AliasIssuedIntegrationEventHandler> logger)
    : IConsumer<AliasIssuedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<AliasIssuedIntegrationEvent> context)
    {
        var msg = context.Message;

        if (msg.AliasId == Guid.Empty || msg.SubjectRef == Guid.Empty)
            throw new ValidationException("AliasId/SubjectRef không hợp lệ.");

        logger.LogInformation("Consuming AliasIssued: {MessageId} alias={AliasId} subjectRef={SubjectRef}",
            context.MessageId, msg.AliasId, msg.SubjectRef);

        var command = new MapAliasOwnerCommand(msg.AliasId, msg.SubjectRef);
        
        await sender.Send(command, context.CancellationToken);
    }
}
