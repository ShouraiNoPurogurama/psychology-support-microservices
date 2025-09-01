using BuildingBlocks.Data.Common;
using BuildingBlocks.Messaging.Events.Profile.Pii;
using Profile.API.Domains.Pii.Dtos;
using Profile.API.Domains.Pii.Features.EnsureSubjectRef;

namespace Profile.API.Domains.Pii.EventHandlers;

public class UserRegisteredIntegrationEventHandler(ISender sender, ILogger<UserRegisteredIntegrationEventHandler> logger)
    : IConsumer<UserRegisteredIntegrationEvent>
{
    public async Task Consume(ConsumeContext<UserRegisteredIntegrationEvent> context)
    {
        var userId = context.Message.UserId;
        
        var seed = new PersonSeed(
            context.Message.FullName,
            context.Message.Gender,
            context.Message.BirthDate,
            new ContactInfo(context.Message.Address, context.Message.Email, context.Message.PhoneNumber)
        );

        var command = new EnsureSubjectRefCommand(userId, seed);

        await sender.Send(command);
    }
}