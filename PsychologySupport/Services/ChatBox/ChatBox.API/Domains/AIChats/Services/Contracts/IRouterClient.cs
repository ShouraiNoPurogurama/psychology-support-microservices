using ChatBox.API.Domains.AIChats.Dtos.AI;

namespace ChatBox.API.Domains.AIChats.Services.Contracts;

public interface IRouterClient
{
    Task<RouterDecisionDto?> RouteAsync(string userMessage, string? history,  CancellationToken ct = default);
}