namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Chatbox;

public record UserMemoryCreatedIntegrationEvent(Guid AliasId, Guid SessionId, string Summary, List<string> Tags, bool SaveNeeded) : IntegrationEvent;