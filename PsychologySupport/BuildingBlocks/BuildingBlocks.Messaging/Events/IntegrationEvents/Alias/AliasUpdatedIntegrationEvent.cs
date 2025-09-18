namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Alias;

public record AliasUpdatedIntegrationEvent(Guid AliasId, Guid SubjectRef, Guid AliasVersionId, string Label, DateTimeOffset ValidFrom) : IntegrationEvent;