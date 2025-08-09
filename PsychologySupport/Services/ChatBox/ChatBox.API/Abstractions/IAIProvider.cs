using ChatBox.API.Dtos.AI;

namespace ChatBox.API.Abstractions;

public interface IAIProvider
{
    Task<string> GenerateResponseAsync(AIRequestPayload payload, Guid sessionId);
}