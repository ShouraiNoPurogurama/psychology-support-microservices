namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Chatbox;

public record SessionCreatedIntegrationEvent(    
    Guid AliasId,
    Guid UserId,
    Guid SessionId) : IntegrationEvent;