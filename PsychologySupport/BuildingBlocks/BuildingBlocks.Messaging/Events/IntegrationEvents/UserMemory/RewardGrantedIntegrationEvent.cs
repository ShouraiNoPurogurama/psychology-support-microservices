namespace BuildingBlocks.Messaging.Events.IntegrationEvents.UserMemory;

public record RewardGrantedIntegrationEvent(Guid RewardId,
    Guid AliasId,
    Guid UserId,
    Guid SessionId,
    string StickerUrl,
    string PromptFilter
    );