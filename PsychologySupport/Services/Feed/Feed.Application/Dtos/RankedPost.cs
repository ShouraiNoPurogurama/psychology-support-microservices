namespace Feed.Application.Dtos;

public record RankedPost(
    Guid PostId,
    Guid AuthorAliasId,
    sbyte RankBucket,
    long RankI64,
    DateTimeOffset CreatedAt
);
