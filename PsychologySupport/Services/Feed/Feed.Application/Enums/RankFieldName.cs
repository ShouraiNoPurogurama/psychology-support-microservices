namespace Feed.Application.Enums;

/// <summary>
/// Redis hash field names as enums to avoid magic strings
/// </summary>
public enum RankFieldName
{
    Score,
    Reactions,
    Comments,
    Ctr,
    UpdatedAt
}
