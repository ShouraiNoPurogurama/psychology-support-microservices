namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Alias;

public record AliasIssuedIntegrationEvent(Guid AliasId, Guid UserId, Guid SubjectRef, Guid AliasVersionId, string Label, DateTimeOffset ValidFrom) : IntegrationEvent;
