using ChatBox.API.Domains.AIChats.Dtos.AI;
using ChatBox.API.Domains.AIChats.Dtos.Gemini;

namespace ChatBox.API.Domains.AIChats.Services.Contracts;

public interface IAIProvider
{
    Task<string> GenerateFineTunedChatResponseAsync(AIRequestPayload payload);
    Task<string> GenerateChatResponseAsync(AIRequestPayload payload, string systemInstruction, CancellationToken ct = default);

    Task<string> CallGeminiStructuredOutputAPIAsync(
        GeminiStructuredOutputRequestDto payload,
        CancellationToken ct = default);

    Task<string> CallGeminiOutputAPIAsync(GeminiRequestDto payload, CancellationToken ct = default);
}