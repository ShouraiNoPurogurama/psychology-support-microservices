namespace UserMemory.API.Shared.Services.Contracts;

public interface IGeminiClient
{
    Task<float[]> EmbedTextAsync(string text, int? outputDim = null, string? taskType = null, CancellationToken ct = default);
    Task<IReadOnlyList<float[]>> EmbedBatchAsync(IEnumerable<string> texts, int? outputDim = null, string? taskType = null, CancellationToken ct = default);
}