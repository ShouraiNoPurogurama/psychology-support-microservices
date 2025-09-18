using BuildingBlocks.Messaging.Events.IntegrationEvents;

namespace Post.Application.Abstractions.Integration.Events;

public sealed record PostMediaAltTextUpdatedIntegrationEvent(
    Guid PostId,
    Guid MediaId,
    string? AltText
) : IntegrationEvent;