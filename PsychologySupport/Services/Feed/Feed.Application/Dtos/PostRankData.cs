namespace Feed.Application.Dtos;

public record PostRankData(
    double Score,
    int Reactions,
    int Comments,
    double Ctr,
    DateTimeOffset UpdatedAt,
    DateTimeOffset CreatedAt,
    Guid AuthorAliasId
);
