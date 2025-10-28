namespace BuildingBlocks.Messaging.Events.IntegrationEvents.UserMemory;

public record RewardFailedIntegrationEvent(
    Guid RewardId,
    Guid AliasId,
    Guid SessionId,
    int PointsToRefund,
    string ErrorMessage
);