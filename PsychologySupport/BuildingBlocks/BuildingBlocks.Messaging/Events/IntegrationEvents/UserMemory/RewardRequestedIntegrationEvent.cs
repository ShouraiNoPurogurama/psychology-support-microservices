namespace BuildingBlocks.Messaging.Events.IntegrationEvents.UserMemory;

public record RewardRequestedIntegrationEvent(Guid RewardId, Guid AliasId, Guid ChatSessionId) : IntegrationEvent;