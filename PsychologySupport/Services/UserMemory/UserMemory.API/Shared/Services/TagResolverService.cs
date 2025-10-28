using Microsoft.EntityFrameworkCore;
using UserMemory.API.Data;
using UserMemory.API.Models;
using UserMemory.API.Shared.Enums;
using UserMemory.API.Shared.Services.Contracts;
using UserMemory.API.Shared.Utils;

namespace UserMemory.API.Shared.Services;

public class TagResolverService(UserMemoryDbContext db) : ITagResolverService
{
    public async Task<IReadOnlyDictionary<string, MemoryTag>> ResolveByCodesAsync(IEnumerable<string> codes, CancellationToken ct)
    {
        var set = codes.Distinct().ToArray();
        var tags = await db.MemoryTags.Where(t => set.Contains(t.Code) && t.IsActive).ToListAsync(ct);
        return tags.ToDictionary(t => t.Code, t => t, StringComparer.Ordinal);
    }

    public async Task<IReadOnlyDictionary<string, MemoryTag>> ResolveByEnumsAsync(
        TopicTag[] topics, EmotionTag[] emotions, RelationshipTag[] relations, CancellationToken ct)
    {
        var codes = topics.Select(x => x.ToCode())
            .Concat(emotions.Select(x => x.ToCode()))
            .Concat(relations.Select(x => x.ToCode()));
        return await ResolveByCodesAsync(codes, ct);
    }
}