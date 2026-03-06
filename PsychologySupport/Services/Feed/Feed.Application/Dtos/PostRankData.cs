using Feed.Application.MessagePacks;

namespace Feed.Application.Dtos;

public record PostRankData(
    double Score,
    int Reactions,
    int Comments,
    double Ctr,
    DateTimeOffset UpdatedAt,
    DateTimeOffset CreatedAt,
    Guid AuthorAliasId,
    int Shares,
    int Clicks,
    int Impressions,
    double ViewDurationSec 
)
{
    public PostRankPack ToPack() => new()
    {
        Score        = Score,
        Reactions    = Reactions,
        Comments     = Comments,
        Ctr          = Ctr,
        UpdatedAtSec = UpdatedAt.ToUnixTimeSeconds(),
        CreatedAtSec = CreatedAt.ToUnixTimeSeconds(),
        AuthorAliasId     = AuthorAliasId,
        Shares         = Shares,
        Clicks        = Clicks,
        Impressions  = Impressions,
        ViewDurationSec = ViewDurationSec
    };
}
