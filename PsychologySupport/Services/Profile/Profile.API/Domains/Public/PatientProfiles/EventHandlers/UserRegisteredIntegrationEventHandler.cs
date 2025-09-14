﻿using BuildingBlocks.Messaging.Events.IntegrationEvents.Auth;
using Profile.API.Domains.Public.PatientProfiles.Features.SeedPatientProfile;

namespace Profile.API.Domains.Public.PatientProfiles.EventHandlers;

public class UserRegisteredIntegrationEventHandler(ISender sender, ILogger<UserRegisteredIntegrationEventHandler> logger)
    : IConsumer<UserRegisteredIntegrationEvent>
{
    public async Task Consume(ConsumeContext<UserRegisteredIntegrationEvent> context)
    {
        var message = context.Message;

        var command = new SeedPatientProfileCommand
        (
            message.SeedProfileId
        );

        var result = await sender.Send(command, context.CancellationToken);

        if (!result.IsSuccess)
        {
            logger.LogError("Failed to create PatientProfile for UserId: {UserId}", context.Message.UserId);
        }
    }
}