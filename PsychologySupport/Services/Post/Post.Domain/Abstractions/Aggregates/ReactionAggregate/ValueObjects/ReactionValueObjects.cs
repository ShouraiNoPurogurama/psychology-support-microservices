using Post.Domain.Exceptions;

namespace Post.Domain.Abstractions.Aggregates.ReactionAggregate.ValueObjects;

public sealed record ReactionTarget
{
    public string TargetType { get; init; }
    public Guid TargetId { get; init; }

    public ReactionTarget(string targetType, Guid targetId)
    {
        if (string.IsNullOrWhiteSpace(targetType))
            throw new InvalidReactionDataException("Loại đối tượng phản ứng không được để trống.");

        if (targetId == Guid.Empty)
            throw new InvalidReactionDataException("ID đối tượng phản ứng không hợp lệ.");

        var validTargetTypes = new[] { "post", "comment" };
        if (!validTargetTypes.Contains(targetType.ToLower()))
            throw new InvalidReactionDataException($"Loại đối tượng phản ứng không hợp lệ. Chỉ chấp nhận: {string.Join(", ", validTargetTypes)}");

        TargetType = targetType.ToLower();
        TargetId = targetId;
    }

    public bool IsPost => TargetType == "post";
    public bool IsComment => TargetType == "comment";
}

public sealed record ReactionType
{
    public string Value { get; init; }
    public string? Emoji { get; init; }
    public int Weight { get; init; }

    public ReactionType(string reactionType)
    {
        if (string.IsNullOrWhiteSpace(reactionType))
            throw new InvalidReactionDataException("Loại phản ứng không được để trống.");

        var validReactions = new Dictionary<string, (string emoji, int weight)>
        {
            { "like", ("👍", 1) },
            { "love", ("❤️", 2) },
            { "haha", ("😂", 1) },
            { "wow", ("😮", 1) },
            { "sad", ("😢", 1) },
            { "angry", ("😡", 1) },
            { "care", ("🤗", 2) },
            { "celebrate", ("🎉", 2) }
        };

        var normalizedType = reactionType.ToLower();
        if (!validReactions.ContainsKey(normalizedType))
            throw new InvalidReactionDataException($"Loại phản ứng không hợp lệ. Chỉ chấp nhận: {string.Join(", ", validReactions.Keys)}");

        Value = normalizedType;
        Emoji = validReactions[normalizedType].emoji;
        Weight = validReactions[normalizedType].weight;
    }

    public bool IsPositive => Value is "like" or "love" or "care" or "celebrate";
    public bool IsNegative => Value is "sad" or "angry";
    public bool IsNeutral => Value is "haha" or "wow";
    public bool IsHighWeight => Weight > 1;
}
