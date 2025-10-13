using BuildingBlocks.Messaging.Events.IntegrationEvents;

namespace Post.Application.Abstractions.Integration.Events;

public sealed record AliasCountersChangedIntegrationEvent(
    Guid AliasId,
    Dictionary<string, int> CountersToUpdate
) : IntegrationEvent;
