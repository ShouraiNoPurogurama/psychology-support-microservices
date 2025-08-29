using Alias.API.Common.Reservations;
using Alias.API.Common.Security;
using Alias.API.Data.Public;
using Alias.API.Domains.Aliases.Dtos;
using Alias.API.Domains.Aliases.Utils;
using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Microsoft.EntityFrameworkCore;

namespace Alias.API.Domains.Aliases.Features.SuggestAliases;

public record SuggestAliasesQuery(PaginationRequest PaginationRequest, TimeSpan Ttl) : IQuery<SuggestAliasesResult>;

public record SuggestAliasesResult(IReadOnlyList<SuggestAliasesItemDto> Aliases, DateTimeOffset GeneratedAt);

public class SuggestAliasesHandler(
    PublicDbContext dbContext,
    IAliasReservationStore store,
    IAliasTokenService tokens)
    : IQueryHandler<SuggestAliasesQuery, SuggestAliasesResult>
{
    private static readonly string[] Animals = ["Gấu", "Mèo", "Cáo", "Thỏ", "Cú", "Hải Ly", "Hổ", "Nhím"];

    public async Task<SuggestAliasesResult> Handle(SuggestAliasesQuery request, CancellationToken ct)
    {
        var need = request.PaginationRequest.PageSize;
        if (need <= 0) need = 1;

        //clamp TTL để tránh client xin TTL quá dài
        var ttl = request.Ttl;
        if (ttl <= TimeSpan.Zero) ttl = TimeSpan.FromSeconds(60);
        if (ttl > TimeSpan.FromMinutes(5)) ttl = TimeSpan.FromMinutes(5);

        var now = DateTimeOffset.UtcNow;
        var expiresAt = now.Add(ttl);

        //Sinh trước gấp "2" lần số tên để tí lọc lại bỏ ra mấy cái bị trùng
        var poolSize = need * 2;
        var candidates = new List<(string Key, string Label)>(poolSize);
        var seenKeys = new HashSet<string>(StringComparer.Ordinal);

        while (candidates.Count < poolSize)
        {
            var label = $"{Animals[Random.Shared.Next(Animals.Length)]} #{Random.Shared.Next(100, 999)}";
            var key = AliasNormalizer.ToKey(label);
            if (seenKeys.Add(key))
                candidates.Add((key, label));
        }

        var keys = candidates.Select(c => c.Key).ToArray();
        var taken = await dbContext.AliasVersions.AsNoTracking()
            .Where(a => a.ValidTo == null && keys.Contains(a.AliasKey))
            .Select(a => a.AliasKey)
            .ToListAsync(ct);

        var takenSet = new HashSet<string>(taken, StringComparer.Ordinal);

        //Lọc còn trống để thử reserve
        var available = candidates.Where(c => !takenSet.Contains(c.Key)).ToList();
        if (available.Count == 0)
            throw new InvalidOperationException("gacha_exhausted");

        //Reserve trên Redis song song để giảm roundtrip tổng
        //(ReserveAsync nên là SET NX EX ttl)
        var reserveTasks = available
            .Select(c => store.ReserveAsync(c.Key, c.Label, ttl, ct))
            .ToArray();

        await Task.WhenAll(reserveTasks);

        //Gom đủ 'need' item đã reserve thành công
        var items = new List<SuggestAliasesItemDto>(need);
        for (int i = 0; i < available.Count && items.Count < need; i++)
        {
            if (!reserveTasks[i].Result) continue;
            var (key, label) = available[i];
            var token = tokens.Create(key, expiresAt);
            items.Add(new SuggestAliasesItemDto(label, token, expiresAt));
        }

        if (items.Count == 0)
            throw new InvalidOperationException("gacha_exhausted");

        return new SuggestAliasesResult(items, now);
    }
}