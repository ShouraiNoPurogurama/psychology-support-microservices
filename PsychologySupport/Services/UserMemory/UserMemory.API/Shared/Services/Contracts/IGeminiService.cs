namespace UserMemory.API.Shared.Services.Contracts;

public interface IGeminiService
{
    Task<string> GenerateTextAsync(string prompt, CancellationToken ct);
}