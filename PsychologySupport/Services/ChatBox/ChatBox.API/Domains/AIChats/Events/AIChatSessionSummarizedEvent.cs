using BuildingBlocks.DDD;
using ChatBox.API.Models;

namespace ChatBox.API.Domains.AIChats.Events;

public record AIChatSessionSummarizedEvent(Guid UserId, Guid SessionId, List<AIMessage> AIMessages) : IDomainEvent; 