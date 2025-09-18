using BuildingBlocks.Messaging.Events.IntegrationEvents.Auth;
using Profile.API.Domains.Pii.Dtos;
using Profile.API.Domains.Pii.Features.SeedPersonProfile;

namespace Profile.API.Domains.Pii.EventHandlers;

public class UserRegisteredIntegrationEventHandler(ISender sender, ILogger<UserRegisteredIntegrationEventHandler> logger)
    : IConsumer<UserRegisteredIntegrationEvent>
{
    public async Task Consume(ConsumeContext<UserRegisteredIntegrationEvent> context)
    {
        var userId = context.Message.UserId;
        
        var seed = new PersonSeedDto(
            context.Message.SeedSubjectRef,
            context.Message.SeedPatientProfileId,
            context.Message.FullName,
            context.Message.Email,
            context.Message.PhoneNumber
        );

        var command = new SeedPersonProfileAndPatientMappingCommand(userId, seed);

        await sender.Send(command);
    }
}