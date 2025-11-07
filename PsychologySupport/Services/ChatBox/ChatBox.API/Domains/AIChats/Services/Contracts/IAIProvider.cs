using ChatBox.API.Domains.AIChats.Dtos.AI;

namespace ChatBox.API.Domains.AIChats.Services.Contracts;

public interface IAIProvider
{
    Task<string> GenerateResponseAsync(AIRequestPayload payload, Guid sessionId);
    Task<string> GenerateResponseAsync_FoundationalModel(AIRequestPayload payload, Guid sessionId, string systemInstruction, CancellationToken ct = default);
}