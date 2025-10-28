using UserMemory.API.Models;
using UserMemory.API.Shared.Enums;

namespace UserMemory.API.Shared.Services.Contracts;

public interface ITagResolverService
{
    Task<IReadOnlyDictionary<string, MemoryTag>> ResolveByCodesAsync(IEnumerable<string> codes, CancellationToken ct);
    Task<IReadOnlyDictionary<string, MemoryTag>> ResolveByEnumsAsync(
        TopicTag[] topics, EmotionTag[] emotions, RelationshipTag[] relations, CancellationToken ct);
}