using BuildingBlocks.DDD;
using ChatBox.API.Models;

namespace ChatBox.API.Events;

public record AIChatSessionSummarizedEvent(Guid UserId, Guid SessionId, List<AIMessage> AIMessages) : IDomainEvent; 