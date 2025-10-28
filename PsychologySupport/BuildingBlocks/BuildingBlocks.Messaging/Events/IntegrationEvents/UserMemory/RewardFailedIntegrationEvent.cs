namespace BuildingBlocks.Messaging.Events.IntegrationEvents.UserMemory;

public record RewardFailedEvent(
    Guid RewardId,
    Guid AliasId,
    string ErrorMessage
);