
namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Alias;

public record AliasVisibilityChangedIntegrationEvent(
    Guid AliasId, 
    string Visibility, 
    DateTimeOffset ChangedAt) : IntegrationEvent;
