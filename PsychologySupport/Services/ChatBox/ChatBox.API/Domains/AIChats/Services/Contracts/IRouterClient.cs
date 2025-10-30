using ChatBox.API.Domains.AIChats.Dtos.AI;

namespace ChatBox.API.Domains.AIChats.Services.Contracts;

public interface IRouterClient
{
    Task<RouterDecisionDto?> RouteAsync(string userMessage, List<HistoryMessage> historyMessages, CancellationToken ct = default);
}