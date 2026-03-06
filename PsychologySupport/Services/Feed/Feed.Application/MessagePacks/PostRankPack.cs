
using Feed.Application.Dtos;

namespace Feed.Application.MessagePacks;

[MessagePack.MessagePackObject]
public class PostRankPack
{
    [MessagePack.Key(0)] public double Score { get; set; }
    [MessagePack.Key(1)] public int Reactions { get; set; }
    [MessagePack.Key(2)] public int Comments { get; set; }
    [MessagePack.Key(3)] public double Ctr { get; set; }
    [MessagePack.Key(4)] public long UpdatedAtSec { get; set; }
    [MessagePack.Key(5)] public long CreatedAtSec { get; set; }
    [MessagePack.Key(6)] public Guid AuthorAliasId { get; set; }
    [MessagePack.Key(7)] public int Shares { get; set; }
    [MessagePack.Key(8)] public int Clicks { get; set; }
    [MessagePack.Key(9)] public int Impressions { get; set; } // Số lượt hiển thị
    [MessagePack.Key(10)] public double ViewDurationSec { get; set; } 
    
    public PostRankData ToPostRankData() => new(
        Score,
        Reactions,
        Comments,
        Ctr,
        UpdatedAtSec > 0 ? DateTimeOffset.FromUnixTimeSeconds(UpdatedAtSec) : DateTimeOffset.UtcNow,
        CreatedAtSec > 0 ? DateTimeOffset.FromUnixTimeSeconds(CreatedAtSec) : DateTimeOffset.UtcNow,
        AuthorAliasId,
        Shares,
        Clicks,
        Impressions,
        ViewDurationSec
    );
}