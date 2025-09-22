using Alias.API.Aliases.Dtos;
using Alias.API.Aliases.Utils;
using Alias.API.Common.Reservations;
using Alias.API.Common.Security;
using Alias.API.Data.Public;
using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;

namespace Alias.API.Aliases.Features.SuggestAliases;

public record SuggestAliasesQuery(PaginationRequest PaginationRequest, TimeSpan Ttl)
    : IQuery<SuggestAliasesResult>;

public record SuggestAliasesResult(IReadOnlyList<SuggestAliasesItemDto> Aliases, DateTimeOffset GeneratedAt);

public class SuggestAliasesHandler(
    AliasDbContext dbContext,
    IAliasReservationStore store,
    IAliasTokenService tokens)
    : IQueryHandler<SuggestAliasesQuery, SuggestAliasesResult>
{
    public async Task<SuggestAliasesResult> Handle(SuggestAliasesQuery request, CancellationToken ct)
    {
        var need = NormalizeNeed(request.PaginationRequest.PageSize);
        var ttl = ClampTtl(request.Ttl);
        var now = DateTimeOffset.UtcNow;
        var expiresAt = now.Add(ttl);

        var candidates = GenerateCandidates(need * 2);
        var takenSet = await GetTakenSetAsync(candidates, ct);
        var available = FilterAvailable(candidates, takenSet);
        EnsureNotEmpty(available);

        var reserveTasks = await ReserveAllAsync(available, ttl, ct);
        var keepIdx = CollectSuccessIndexes(reserveTasks, need);
        var extraIdx = CollectExtraIndexes(reserveTasks, keepIdx);

        ReleaseExtras(available, extraIdx, ct); // fire-and-forget

        var items = BuildItems(available, keepIdx, expiresAt);
        EnsureNotEmpty(items);

        return new SuggestAliasesResult(items, now);
    }

    private static int NormalizeNeed(int pageSize) => pageSize <= 0 ? 1 : pageSize;

    private static TimeSpan ClampTtl(TimeSpan ttl)
    {
        if (ttl <= TimeSpan.Zero) return TimeSpan.FromSeconds(60);
        if (ttl > TimeSpan.FromMinutes(5)) return TimeSpan.FromMinutes(5);
        return ttl;
    }

    private static List<(string Key, string Label)> GenerateCandidates(int poolSize)
        => AliasNamesUtils.GenerateCandidates(poolSize);

    private async Task<HashSet<string>> GetTakenSetAsync(
        List<(string Key, string Label)> candidates,
        CancellationToken ct)
    {
        var keys = candidates.Select(c => c.Key).ToArray();
        var taken = await dbContext.AliasVersions.AsNoTracking()
            .Where(a => a.ValidTo == null && keys.Contains(a.UniqueKey))
            .Select(a => a.UniqueKey)
            .ToListAsync(ct);

        return new HashSet<string>(taken, StringComparer.Ordinal);
    }

    private static List<(string Key, string Label)> FilterAvailable(
        List<(string Key, string Label)> candidates,
        HashSet<string> takenSet)
        => candidates.Where(c => !takenSet.Contains(c.Key)).ToList();

    private static void EnsureNotEmpty<T>(ICollection<T> collection)
    {
        if (collection.Count == 0) throw new InvalidOperationException("gacha_exhausted");
    }

    private async Task<Task<bool>[]> ReserveAllAsync(
        List<(string Key, string Label)> available,
        TimeSpan ttl,
        CancellationToken ct)
    {
        var tasks = available
            .Select(c => store.ReserveAsync(c.Key, c.Label, ttl, ct))
            .ToArray();

        await Task.WhenAll(tasks);
        return tasks;
    }

    private static int[] CollectSuccessIndexes(Task<bool>[] reserveTasks, int need)
    {
        var successes = new List<int>(reserveTasks.Length);
        for (int i = 0; i < reserveTasks.Length; i++)
            if (reserveTasks[i].Result)
                successes.Add(i);

        if (successes.Count == 0)
            throw new InvalidOperationException("gacha_exhausted");

        var take = Math.Min(need, successes.Count);
        return successes.Take(take).ToArray();
    }

    private static int[] CollectExtraIndexes(Task<bool>[] reserveTasks, int[] keepIdx)
    {
        var keep = new HashSet<int>(keepIdx);
        var extra = new List<int>();
        for (int i = 0; i < reserveTasks.Length; i++)
            if (reserveTasks[i].Result && !keep.Contains(i))
                extra.Add(i);
        return extra.ToArray();
    }

    private void ReleaseExtras(
        List<(string Key, string Label)> available,
        int[] extraIdx,
        CancellationToken ct)
    {
        foreach (var idx in extraIdx)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await store.RemoveAsync(available[idx].Key, CancellationToken.None);
                }
                catch
                {
                    //ignore lỗi Redis/network
                }
            }, ct);
        }
    }


    private List<SuggestAliasesItemDto> BuildItems(
        List<(string Key, string Label)> available,
        int[] keepIdx,
        DateTimeOffset expiresAt)
    {
        var items = new List<SuggestAliasesItemDto>(keepIdx.Length);
        foreach (var idx in keepIdx)
        {
            var (key, label) = available[idx];
            var token = tokens.Create(key, expiresAt);
            items.Add(new SuggestAliasesItemDto(label, token, expiresAt));
        }

        return items;
    }
}