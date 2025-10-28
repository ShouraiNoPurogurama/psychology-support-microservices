namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Chatbox;

/// <summary>
/// Published by ChatBox.API after every user message is processed by AI.
/// Consumed by UserMemory.API to track daily progress points.
/// </summary>
public record MessageProcessedIntegrationEvent(
    Guid AliasId,
    Guid UserId,
    Guid SessionId,
    string UserMessage,
    bool SaveNeeded,
    List<string> Tags,
    DateTimeOffset ProcessedAt
) : IntegrationEvent;
