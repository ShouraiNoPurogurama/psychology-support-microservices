using ChatBox.API.Domains.AIChats.Dtos.AI;
using ChatBox.API.Models;

namespace ChatBox.API.Domains.AIChats.Services.Contracts;

public interface IAIRequestFactory
{
    Task<AIRequestPayload> CreateAsync(
        List<HistoryMessage> historyMessages,
        AIChatSession session,
        string augmentedContext);
}