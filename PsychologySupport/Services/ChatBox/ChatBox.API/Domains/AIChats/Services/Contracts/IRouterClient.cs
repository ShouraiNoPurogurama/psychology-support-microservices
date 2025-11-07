using ChatBox.API.Domains.AIChats.Dtos.AI;
using ChatBox.API.Domains.AIChats.Dtos.AI.Router;

namespace ChatBox.API.Domains.AIChats.Services.Contracts;

public interface IRouterClient
{
    Task<RouterDecisionDto?> RouteAsync(string userMessage, List<HistoryMessage> historyMessages, CancellationToken ct = default);
}