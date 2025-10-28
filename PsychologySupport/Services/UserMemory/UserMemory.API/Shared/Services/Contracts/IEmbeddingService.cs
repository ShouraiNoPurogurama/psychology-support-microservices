using UserMemory.API.Shared.Enums;

namespace UserMemory.API.Shared.Services.Contracts;

using UserMemory = Models.UserMemory;

public interface IEmbeddingService
{
    Task<UserMemory> UpsertMemoryAsync(Guid aliasId, 
        string summary, 
        string[] tagCodes, 
        CancellationToken ct = default);

    Task<IReadOnlyList<UserMemory>> AddMemoriesBatchAsync(Guid aliasId, IEnumerable<(string summary, string[] tagCodes)> items,
        CancellationToken ct = default);

    Task<IReadOnlyList<(UserMemory row, double score)>> SearchAsync(Guid aliasId, string query, int topK = 10,
        CancellationToken ct = default);
}